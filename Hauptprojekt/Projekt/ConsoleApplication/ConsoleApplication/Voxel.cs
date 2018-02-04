using System;

namespace Werkzeugbahnplanung
{
   public class Voxel
    {
        private bool m_Schichtrand;
        private bool m_Modellrand;
        private ushort[] m_koordinaten;
        private double[] m_orientierung;

        //Konstruktoren
        public Voxel()
        {
            m_Schichtrand = false;
            m_Modellrand = false;
            m_koordinaten = new ushort[3]{0,0,0};
            m_orientierung = new double[3]{0.0,0.0,0.0};
        }

        public Voxel(bool schichtrand, bool modellrand, ushort xKoord, ushort yKoord, ushort zKoord)
        {
            m_Schichtrand = schichtrand;
            m_Modellrand = modellrand;
            m_koordinaten = new ushort[3];
            m_koordinaten[0] = xKoord;
            m_koordinaten[1] = yKoord;
            m_koordinaten[2] = zKoord;
            m_orientierung = new double[3]{0.0,0.0,0.0};
        }
      
         public Voxel(bool schichtrand, bool modellrand, ushort xKoord, ushort yKoord, ushort zKoord, double xOrient, double yOrient, double zOrient )
        {
            m_Schichtrand = schichtrand;
            m_Modellrand = modellrand;
            m_koordinaten = new ushort[3];
            m_orientierung = new double[3];
            m_koordinaten[0] = xKoord;
            m_koordinaten[1] = yKoord;
            m_koordinaten[2] = zKoord;
            m_orientierung[0] = xOrient;
            m_orientierung[1] = yOrient;
            m_orientierung[2] = zOrient;
        }
        
        public Voxel(ushort[] koordinaten)
        {
            m_Schichtrand = false;
            m_Modellrand = false;
            m_koordinaten = koordinaten;
            m_orientierung = new double[3]{0.0,0.0,0.0};
        }
        //Getter             
        public bool getSchichtrand()
        {
            return m_Schichtrand;
        }

        public bool getModellrand()
        {
            return m_Modellrand;
        }

        public ushort[] getKoords()
        {
            return m_koordinaten;
        }
      
        public double[] getOrientierung() {
            return m_orientierung;
        }

        public double getOrientierungAt(int index)
        {
            return m_orientierung[index];
        }


        //Setter
        public void setSchichtrand(bool value)
        {
            m_Schichtrand = value;
        }

        public void setModellrand(bool value)
        {
            m_Modellrand = value;
        }
        
        //Prüfe ob zwei Voxel über Ecken, Kanten und Flächen benachbart sind
        public bool IsNeighbor26(Voxel a)
        {
            if (a != null) //der betrachtetze Voxel darf nicht leer sein
            {
                int[] distanz = this.VoxelKoordinatenDistanz(a); 
                int distanzSumme = distanz[0] + distanz[1] + distanz[2];
               /*damit zwei Voxel benachbart sind, müssen die Koordinaten jeweils höchstens um eins unterschiedlich sein
                und deren Summe darf nicht null sein, weil ein Voxel kein Nachbar von sich selbst ist. */
                if ((distanz[0] <= 1 && distanz[1] <= 1 && distanz[2] <= 1)&&(distanzSumme != 0))
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
         
         //Prüft oft zwei Voxel über Flächen benachbart sind.
        public bool IsNeighbor6(Voxel a)
        {
            if (a != null) //der betrachtete Voxel darf nicht leer sein
            {
                int[] distanz = this.VoxelKoordinatenDistanz(a);
               /*Die Distanz zwischen beiden Voxeln muss eine Koordinate in eine einzige Richtung betragen.*/
                if ((distanz[0] == 0 && distanz[1] == 0 && distanz[2] == 1) ||
                    (distanz[0] == 0 && distanz[1] == 1 && distanz[2] == 0) ||
                    (distanz[0] == 1 && distanz[1] == 0 && distanz[2] == 0))
                    return true;
                return false;
            }
            else
                return false;
        }

        // Erstelle ein Distanzarray der einzelnen Voxelkoordinaten für die Abstandsberechnung
        public int[] VoxelKoordinatenDistanz(Voxel a)
        {
            int[] distanz = new int[3] {0,0,0};
            distanz[0] = Math.Abs(this.getKoords()[0] - a.getKoords()[0]);
            distanz[1] = Math.Abs(this.getKoords()[1] - a.getKoords()[1]);
            distanz[2] = Math.Abs(this.getKoords()[2] - a.getKoords()[2]);
            return distanz;
        }
    }
}
