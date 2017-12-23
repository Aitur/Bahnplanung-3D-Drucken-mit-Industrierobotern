using Werkzeugbahnplanung;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Werkzeugbahnplanung
{
    public class Voxelmodell
    {
        private int m_AnzahlSchichten;
        private Infill m_Boundingbox;
        //3-D Voxelmodell
        private Voxel[,,] m_Voxelmatrix;
        //Liste auf die entsprechenden Schichten-Listen
        private List<List<Voxel>> m_Schichten;

        //Konstruktor für Input-Funktion vorgesehen
        public Voxelmodell(int anzahlSchichten,Voxel[,,] voxelmatrix, List<List<Voxel>> schichten, int infillDensity = 20, string infillType = "3DInfill")
        {
            m_AnzahlSchichten = anzahlSchichten;
            m_Boundingbox = new Infill(infillDensity, infillType);
            m_Voxelmatrix = voxelmatrix;
            m_Schichten = schichten;
        }
        /// <summary>
        /// lediglich benötigt, um die Randverbreiterung einfacher zu testen
        /// </summary>
        /// <returns></returns>
        public Voxel[,,] getVoxelmatrix()
        {
            return m_Voxelmatrix;
        }
         public List<List<Voxel>> Schichten()
        {
            return m_Schichten;
        }

        //Getter
        public int getSchichtenAnzahl()
        {
            return m_AnzahlSchichten;
        }

        public List<Voxel> getListeAtIndex(int i)
        {
            return m_Schichten[i];
        }
        
        #region InsertInfill
        /*Funktion, die eine Boundingbox eines Infill-Musters
          (derselben Größe(!)) mit dem Voxelmodell merged.
          Bounding-Box : true = Voxel gesetzt im Infill */
        public void InsertInfill()
        {
            ushort[] koords = new ushort[3];
            //Schleifen die über alle Voxel des Modells gehen
            foreach (List<Voxel> schicht in m_Schichten)
            {
                for (int i = 0; i < schicht.Count(); i++)
                {
                    //Voxel die Teil des Randes sind kommen nicht in Frage
                    if (schicht[i].getModellrand() != true) //#Question: Modellrand anstatt Schichtrand?
                    {
                        koords = schicht[i].getKoords();
                        //Falls kein Infill an Stelle des Voxels, lösche diesen
                        //aus unserem Voxelmodell
                        if (0 == m_Boundingbox.IsInfill(koords[0], koords[1], koords[2]))
                        {
                            m_Voxelmatrix[koords[0], koords[1], koords[2]] = null;
                            schicht.Remove(schicht[i]);
                            i--;
                        }
                    }
                }
            }
        }
        #endregion
    
#region Randverbreiterung
        /// <summary>
        /// Diese Methode ist dazu da, um den Modellrand (voll zu druckenden äußeren Bereich) verbreitern.
        /// Der Parameter randBreite soll die gewünschte resultierende Breite dieses Bereiches sein.
        /// </summary>
        /// <param name="randBreite"></param>
        public void randVerbreiterung(int randBreite)
        {
            //Voxel werden erst in eine Liste eingetragen, bevor sie zum Rand hinzugefügt werden
            List<ushort[]> hinzufügendeVoxel = new List<ushort[]>();
            /*Es werden immer benachbarte Voxel hinzugefügt.
            Um die gewünschte Breite zu erreichen, muss der Vorgang entsprechend oft durchgeführt werden.*/
            for (int i = 0; i < randBreite - 1; i++)
            {
                //Die Verwendung einer foreach Schleife ist hier nicht möglich, da die Koodidaten des zu betrachtenden Voxels benötigt werden.
                foreach (Voxel voxel in m_Voxelmatrix)
                {
                    /*Es sind nur Voxel relevant, die zum Rand gehören.
                    Leere Voxel müssen vorher aussortiert werden, weil wir nicht auf Attribute leerer Objekte zugreifen können.
                    Die zweite Bedingung wird nicht ausgeführt, wenn die erste nicht erfüllt ist -> Fehlervermeidung*/
                    if (voxel != null && voxel.getModellrand() == true)
                    {
                        ushort x = voxel.getKoords()[0];
                        ushort y = voxel.getKoords()[1];
                        ushort z = voxel.getKoords()[2];
                        /*Jeder Voxel hat 6, 18 oder 26 Nachbarn, je nach dem ob man Nachbarschaften über Flächen, Kanten und/oder Ecken als Nachbarschaft ansieht.
                         Die Programmierung ist auf 26 ausgelegt.
                         Bei jedem möglichen Nachbar ist zu prüfen, ob dieser im Modell liegt(nicht out of range) und existiert (nicht null)
                         Die Prüfung wurde in eine neue Methode ausgelagert. Sie wird mit allen Voxeln aufgerunfen, bei denen sich jede Koodinate, um max 1 unterscheiden.*/
                        foreach(Voxel v in getNeighbors(voxel))
                        {
                            if(v.getModellrand() == false)
                            {
                                hinzufügendeVoxel.Add(v.getKoords());
                            }
                        }
                    }
                }
                //Damit das Verändern von Voxeln sich erst auf die nächste Iteration auswirkt, dürfen die Voxel nicht direkt verändert werden
                foreach (ushort[] pos in hinzufügendeVoxel)
                {
                    m_Voxelmatrix[pos[0], pos[1], pos[2]].setModellrand(true);
                }
                hinzufügendeVoxel.Clear();
            }
        }
        /// <summary>
        /// Diese Methode übergibt alle existierenden Nachbarn eines existierenden Voxels
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public List<Voxel> getNeighbors(Voxel a)
        {
            int x = a.getKoords()[0];
            int y = a.getKoords()[1];
            int z = a.getKoords()[2];
            List<Voxel> nachbarn = new List<Voxel>();
            for (int x_div = -1; x_div <= 1; x_div++)
            {
                for (int y_div = -1; y_div <= 1; y_div++)
                {
                    for (int z_div = -1; z_div <= 1; z_div++)
                    {
                        int neighbor_x = x + x_div;
                        int neighbor_y = y + y_div;
                        int neighbor_z = z + z_div;
                        if (neighbor_x < 0 || neighbor_y < 0 || neighbor_z < 0 || neighbor_x > m_Voxelmatrix.GetUpperBound(0) || neighbor_y > m_Voxelmatrix.GetUpperBound(1) || neighbor_z > m_Voxelmatrix.GetUpperBound(2))
                        {
                            continue;
                        }
                        else if (a.IsNeighbor26(m_Voxelmatrix[x + x_div, y + y_div, z + z_div]))//nicht null)
                        {
                            nachbarn.Add(m_Voxelmatrix[x + x_div, y + y_div, z + z_div]);
                        }

                    }
                }
            }
            return nachbarn;
        }
        #endregion
    }
   
}
