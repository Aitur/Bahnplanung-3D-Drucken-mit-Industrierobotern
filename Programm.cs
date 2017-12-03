using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Werkzeugbahnplanung
{
    class Programm
    {
        static void Main(string[] args)
        {
            Voxelmodell v = Input("Test3.txt");
            randverbreiterungtesten(v);

        }
        #region Input-Method
        /*Funktion für eine Input-Textdatei
          entsprechend der Einigung
          1. Zeile : Grundwerte Voxelmodell
          2.-N. Zeile: Voxel
          Als Ausgabewert wird das fertige Voxelmodell
          zurückgegeben 
          UpdateA: Vor der ersten Zeile können nun (für übersichtliche Testdateien) auch Kommentare stehen
          Wie bisher funktionieren Kommentare zwischen den Eingabezeilen*/
        public static Voxelmodell Input(string filename)
        {
            //Diese Flags sind notwendig, um mehrere Zeilen zu lesen, bevor die Eingabe anfängt
            bool readfirstline = false;
            int firstline= 0;
            //Erstelle Dateipfad und checke ob Datei vorhanden
            var path = Path.Combine(Directory.GetCurrentDirectory(), filename);
            if (!File.Exists(path)) { return null; }
            //Speichere Zeile für Zeile die Daten ab
            string[] lines = System.IO.File.ReadAllLines(@path);
            int boundB_x = 0, boundB_y = 0, boundB_z = 0, anzSchichten = 0;
            List<List<Voxel>> schichten = new List<List<Voxel>>();
            //Versuche Werte aus der 1. Zeile zu parsen ansonsten fahre mit den nächsten Zeilen fort
            while(! readfirstline)
            {
                string[] tokens = lines[firstline].Split(' ');
                if (Int32.TryParse(tokens[0], out boundB_x) &&
                Int32.TryParse(tokens[1], out boundB_y) &&
                Int32.TryParse(tokens[2], out boundB_z) &&
                Int32.TryParse(tokens[3], out anzSchichten))
                {
                    readfirstline = true; //Es werden keine weiteren Zeilen probiert
                    //Erstelle für jede Schicht eine eigene Liste
                    for (int i = 0; i < anzSchichten; i++)
                    {
                        schichten.Add(new List<Voxel>());
                    }
                    //Erstelle Voxelmatrix entsprechend den Dimensionen des Modells
                    Voxel[,,] voxelmatrix = new Voxel[boundB_x, boundB_y, boundB_z];
                    //Schleife über alle Zeilen(bis auf die erste)
                    for (int i = firstline+1; i < lines.Length; i++)
                    {
                        string[] voxelparam = lines[i].Split(' ');
                        ushort[] voxelKoords = new ushort[3];
                        int schicht = 0;
                        //Für die Anzeige im Gnuplot werden werden ushorts benötigt. Diese können ohne Probleme später in booleans umgewandelt werden
                        ushort schichtrand = 0, modellrand = 0;
                        //Versuche alle Werte der Zeile zu parsen ansonsten gehe zur nächsten Zeile
                        if (ushort.TryParse(voxelparam[0], out voxelKoords[0]) &&
                            ushort.TryParse(voxelparam[1], out voxelKoords[1]) &&
                            ushort.TryParse(voxelparam[2], out voxelKoords[2]) &&
                            ushort.TryParse(voxelparam[3], out schichtrand) &&
                            ushort.TryParse(voxelparam[4], out modellrand) &&
                            Int32.TryParse(voxelparam[5], out schicht))
                        {
                            //Erstelle anhand der Werte einen neuen Voxel in der Matrix 
                            //und in der entsprechenden Schicht
                            voxelmatrix[voxelKoords[0], voxelKoords[1], voxelKoords[2]] = new Voxel(Convert.ToBoolean(schichtrand),Convert.ToBoolean(modellrand), voxelKoords[0], voxelKoords[1], voxelKoords[2]);
                            schichten[schicht].Add(voxelmatrix[voxelKoords[0], voxelKoords[1], voxelKoords[2]]);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    //Erstellt Voxelmodell wenn Datei vollkommen durchlaufen und gebe diese zurück
                    Voxelmodell voxelmodell = new Voxelmodell(anzSchichten, boundB_x, boundB_y, boundB_z, voxelmatrix, schichten);
                    return voxelmodell;
                }
                else
                {
                    firstline++;
                }
            } //Falls keine Zeile dem gewünschten Format entspricht, wird abgebrochen
            return null;
        }
        #endregion
        #region Tests
        public static void randverbreiterungtesten(Voxelmodell voxelmodell)
        {
            Voxelmodell voxelmodell1 = voxelmodell;
            voxelmodell1.randVerbreiterung(1);
            using (StreamWriter file = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "randverbreiterung1.txt")))
            {
                file.WriteLine("#Randverbreiterung mit 1");
                foreach(Voxel v in voxelmodell1.getVoxelmatrix())
                {
                    file.WriteLine(v.getKoords()[0].ToString() + " " + v.getKoords()[1].ToString() + " " + v.getKoords()[2].ToString() + " " + Convert.ToInt16(v.getModellrand()).ToString());
                }
            }
            Voxelmodell voxelmodell2 = voxelmodell;
            voxelmodell1.randVerbreiterung(2);
            using (StreamWriter file = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "randverbreiterung2.txt")))
            {
                file.WriteLine("#Randverbreiterung mit 1");
                foreach (Voxel v in voxelmodell1.getVoxelmatrix())
                {
                    file.WriteLine(v.getKoords()[0].ToString() + " " + v.getKoords()[1].ToString() + " " + v.getKoords()[2].ToString() + " " + Convert.ToInt16(v.getModellrand()).ToString());
                }
            }
            Voxelmodell voxelmodell3 = voxelmodell;
            voxelmodell1.randVerbreiterung(3);
            using (StreamWriter file = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "randverbreiterung3.txt")))
            {
                file.WriteLine("#Randverbreiterung mit 1");
                foreach (Voxel v in voxelmodell1.getVoxelmatrix())
                {
                    file.WriteLine(v.getKoords()[0].ToString() + " " + v.getKoords()[1].ToString() + " " + v.getKoords()[2].ToString() + " " + Convert.ToInt16(v.getModellrand()).ToString());
                }
            }
        }
        #endregion
    }
}
