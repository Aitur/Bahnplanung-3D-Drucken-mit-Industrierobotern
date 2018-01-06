using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace Werkzeugbahnplanung
{
    class Programm
    {
        static void Main(string[] args)
        {
            /*
             * Konstanten
             */            
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            string inputFile = @"Galgen.txt";
            int randBreite = 3;
            string currentPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            string path = currentPath+ "\\";
            string fileName = @"Bahnplanung.txt";
            int infillDensity = 20;
            string infillType = "HexInfill"; //3DInfill oder HexInfill
            double robotGeschwindigkeit = 30.0;
            double extrusionsGeschwindigkeit = 36.0;
            /*
             * Konstanten
             */
            
            Console.WriteLine("Lese Modell ein...");
            Voxelmodell v = Input(path + inputFile);
            Console.WriteLine("Modell eingelesen!");
            
            Console.WriteLine("Verbreitere Rand...");
            v.randVerbreiterung(randBreite);
            Console.WriteLine("Rand verbreitert!");
            
            Console.WriteLine("Füge das Infill ein...");
            v.InsertInfill(infillDensity,infillType);
            Console.WriteLine("Infill eingefügt!");     
            
            Console.WriteLine("Plane die Bahn...");    
            Bahn bahn = new Bahn();
            if (File.Exists(path + fileName))
                File.Delete(path + fileName);
            for (int i = 0; i < v.getSchichtenAnzahl(); i++)
            {
                Console.WriteLine("Plane die Bahn für Schicht " + i + "/" + v.getSchichtenAnzahl());
                bahn.Bahnplanung(v.getListeAtIndex(i), robotGeschwindigkeit, extrusionsGeschwindigkeit, path, fileName, (i+1));
            }
            Console.WriteLine("Bahn geplant!");
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
                Int32.TryParse(tokens[3], out anzSchichten) )
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
                        double[] voxelOrientierung = new double[3];
                        int schicht = 0;
                        //Für die Anzeige im Gnuplot werden werden ushorts benötigt. Diese können ohne Probleme später in booleans umgewandelt werden
                        ushort schichtrand = 0, modellrand = 0;
                        //Versuche alle Werte der Zeile zu parsen ansonsten gehe zur nächsten Zeile
                        if (ushort.TryParse(voxelparam[0], out voxelKoords[0]) &&
                            ushort.TryParse(voxelparam[1], out voxelKoords[1]) &&
                            ushort.TryParse(voxelparam[2], out voxelKoords[2]) &&
                            ushort.TryParse(voxelparam[6], out schichtrand) &&
                            ushort.TryParse(voxelparam[7], out modellrand) &&
                            Int32.TryParse(voxelparam[8], out schicht) &&
                            double.TryParse(voxelparam[3], NumberStyles.Any, CultureInfo.InvariantCulture, out voxelOrientierung[0]) &&
                            double.TryParse(voxelparam[4], NumberStyles.Any, CultureInfo.InvariantCulture, out voxelOrientierung[1]) &&
                            double.TryParse(voxelparam[5], NumberStyles.Any, CultureInfo.InvariantCulture, out voxelOrientierung[2]))
                        {
                            //Erstelle anhand der Werte einen neuen Voxel in der Matrix 
                            //und in der entsprechenden Schicht
                            voxelmatrix[voxelKoords[0], voxelKoords[1], voxelKoords[2]] = new Voxel(Convert.ToBoolean(schichtrand), Convert.ToBoolean(modellrand), voxelKoords[0], voxelKoords[1], voxelKoords[2], voxelOrientierung[0], voxelOrientierung[1], voxelOrientierung[2]);
                            schichten[schicht - 1].Add(voxelmatrix[voxelKoords[0], voxelKoords[1], voxelKoords[2]]);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    //Erstellt Voxelmodell wenn Datei vollkommen durchlaufen und gebe diese zurück
                    Voxelmodell voxelmodell = new Voxelmodell(anzSchichten, voxelmatrix, schichten);
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
        /// <summary>
        /// Testet Randverbreiterung; Funktioniert nicht mit null Werten...
        /// </summary>
        /// <param name="voxelmodell"></param>
        public static void randverbreiterungtesten(Voxelmodell voxelmodell)
        {
            Voxelmodell voxelmodell1 = voxelmodell;
            voxelmodell1.randVerbreiterung(1);
            using (StreamWriter file = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "randverbreiterung1.txt")))
            {
                file.WriteLine("#Randverbreiterung mit 1");
                foreach(Voxel v in voxelmodell1.getVoxelmatrix())
                {
                    if(v != null)
                    {
                        file.WriteLine(v.getKoords()[0].ToString() + " " + v.getKoords()[1].ToString() + " " + v.getKoords()[2].ToString() + " " + Convert.ToInt16(v.getModellrand()).ToString());
                    }
                }
            }
            Voxelmodell voxelmodell2 = voxelmodell;
            voxelmodell1.randVerbreiterung(2);
            using (StreamWriter file = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "randverbreiterung2.txt")))
            {
                file.WriteLine("#Randverbreiterung mit 1");
                foreach (Voxel v in voxelmodell1.getVoxelmatrix())
                {
                    if (v != null)
                    {
                        file.WriteLine(v.getKoords()[0].ToString() + " " + v.getKoords()[1].ToString() + " " + v.getKoords()[2].ToString() + " " + Convert.ToInt16(v.getModellrand()).ToString());
                    }
                }
            }
            Voxelmodell voxelmodell3 = voxelmodell;
            voxelmodell1.randVerbreiterung(3);
            using (StreamWriter file = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "randverbreiterung3.txt")))
            {
                file.WriteLine("#Randverbreiterung mit 1");
                foreach (Voxel v in voxelmodell1.getVoxelmatrix())
                {
                    if (v != null)
                    {
                        file.WriteLine(v.getKoords()[0].ToString() + " " + v.getKoords()[1].ToString() + " " + v.getKoords()[2].ToString() + " " + Convert.ToInt16(v.getModellrand()).ToString());
                    }
                }
            }
        }
        /// <summary>
        /// Testet Die Mustereinprägung in das Modell. Eingestellt auf 5*5*5 Muster.
        /// </summary>
        /// <param name="voxelmodell"></param>
        public static void testeMuster()
        {
            int x = 30, y = 30, z = 30;
            List<List<Voxel>> schichten = new List<List<Voxel>>();
            schichten.Add(new List<Voxel>());
            Voxel[,,] bb = new Voxel[x, y, z];
            for (ushort i = 0; i < x; i++) {
                for (ushort j = 0; j < x; j++)
                {
                    for (ushort k = 0; k < x; k++)
                    {
                        Voxel v = new Voxel(false, false, i, j, k);
                        bb[i, j, k] = v;
                        schichten[0].Add(v);
                    }
                }
            }
            Voxelmodell voxelmodell1 = new Voxelmodell(0, bb, schichten); ;
            voxelmodell1.InsertInfill();
            Voxel[,,] matrix = voxelmodell1.getVoxelmatrix();
            using (StreamWriter file = new StreamWriter(Path.Combine(Directory.GetCurrentDirectory(), "mustererzeugung.txt")))
            {
                file.WriteLine("#Muster mit %2 test");
                for (int i = 0; i < x; i++)
                {
                    for (int j = 0; j < y; j++)
                    {
                        for (int k = 0; k < z; k++)
                        {
                            string c;
                            if (matrix[i, j, k] == null)
                            {
                                c = "0"; //markiert leerer voxel
                            }
                            else
                            {
                                c = "1"; //leerer voxel
                                file.WriteLine(i + " " + j + " " + k + " " + c);
                            }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
