using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Werkzeugbahnplanung
{
    public class Bahn
    {
        /*
         * Bahnobjekt mit Parametern
         */
        private List<List<Voxel>> m_splitList;
        private Graph_ m_randGraph;
        private Graph_ m_restGraph;
        private Druckfolge m_optimizedRand;
        private Druckfolge m_optimizedRest;
        private int m_layerIndex;

        //Konstruktoren
        public Bahn(){}
        public Bahn(List<List<Voxel>> splitList,
                    Graph_ randGraph,
                    Graph_ restGraph,
                    Druckfolge optimizedRand,
                    Druckfolge optimizedRest,
                    int layerIndex)
        {
            m_splitList = splitList;
            m_randGraph = randGraph;
            m_restGraph = restGraph;
            m_optimizedRand = optimizedRand;
            m_optimizedRest = optimizedRest;
            m_layerIndex = layerIndex;
        }
        
        //Setter
        public void SetBahn(Bahn bahn)
        {
            m_splitList = bahn.m_splitList;
            m_randGraph = bahn.m_randGraph;
            m_restGraph = bahn.m_restGraph;
            m_optimizedRand = bahn.m_optimizedRand;
            m_optimizedRest = bahn.m_optimizedRest;
            m_layerIndex = bahn.m_layerIndex;
        }
        
        //Getter
        public int GetLayerIndex()
        {
            return m_layerIndex;
        }
        
        
        
        //Festlegen von Absetzkosten 
        private const int ABSETZKOSTEN = 20;
        private const int MARKIERKOSTEN = 100;               

        public List<List<Voxel>> SplitVoxelList(List<Voxel> voxelList)
        {
            List<Voxel> voxelListEins = new List<Voxel>();           
            List<Voxel> voxelListZwei = new List<Voxel>(); 
            
            foreach (var v in voxelList)
            {
                if (v.getSchichtrand() == true)
                    voxelListEins.Add(v);
                else
                    voxelListZwei.Add(v);
            }
            List<List<Voxel>> splitList = new List<List<Voxel>>();
            splitList.Add(voxelListEins);
            splitList.Add(voxelListZwei);
            return splitList;
        }


        public double EudklidDistanzAusVoxelDistanz(int[] distanz)
        {
            return (Math.Sqrt(Math.Pow(distanz[0], 2) + 
                              Math.Pow(distanz[1], 2) + 
                              Math.Pow(distanz[2], 2)));
        }
        /*
         * Umwandlung einer Voxelschicht zu einem Graphen. Anhand von Nachbarschaften werden kosten für die Kanten festgelegt, und absetzpunkte werden definiert.
         * Ein Absetzpunkt ensteht immer wenn ein Voxel nicht direkt zu einem anderen Voxel benachbart ist.
         */
        private Graph_ VoxelToGraph(List<Voxel> voxel, bool isInfill)
        {       
            Graph_ graph = new Graph_();
            
            foreach (var v in voxel)
            {
                List<double> graphElemente = new List<double>();
                foreach (var w in voxel)
                {
                    int[] distanz = new int[3] {0, 0, 0};
                    distanz = v.VoxelKoordinatenDistanz(w);                   
                    /*
                     * Berechne für alle Benachbarten und nicht benachbarten Knoten jeden Knotens, die Distanz
                     * zu den anderen Knoten. Füge die VoxelKoordinaten hinzu. Nachbarschaftsfunktion je nach Druckwunsch auswählen
                    */
                    if (isInfill)
                    {
                        if (v.IsNeighbor26(w))                  
                            graphElemente.Add(EudklidDistanzAusVoxelDistanz(distanz));                   
                        else                 
                            graphElemente.Add(ABSETZKOSTEN + EudklidDistanzAusVoxelDistanz(distanz));
                    }
                    else
                    {
                        if (v.IsNeighbor6(w))                  
                            graphElemente.Add(EudklidDistanzAusVoxelDistanz(distanz));                   
                        else                 
                            graphElemente.Add(ABSETZKOSTEN + EudklidDistanzAusVoxelDistanz(distanz));
                    }
                                     
                }
                graph.AddGraphElement(graphElemente);             
                graph.AddVoxelKoordinaten(v.getKoords());
            }
            MarkiereEigenknoten(graph); 
            return graph;
        }

        /*
         * Markiert die Kante eines Knotens zu sich selbst, unter Berücksichtigung
         * der Matrixeigenschaft der Liste von Listen.
         */
        private Graph_ MarkiereEigenknoten(Graph_ graph)
        {
            for (int i = 0; i < graph.GetGraph().Count; i++)
            {
                graph.SetGraph(graph.GetGraphElement(i, i) + MARKIERKOSTEN, i, i);
            }
            return graph;
        }
        
        //Markiert einen Knoten, für alle anderen Knoten in den Kostenlisten
        private static Graph_ MarkiereKnoten(int knoten, Graph_ graph)
        {
            for (int i = 0; i < graph.GetGraph().Count; i++)
            {
                graph.SetGraph(graph.GetGraphElement(i, knoten) + MARKIERKOSTEN, i, knoten);
            };
            graph.SetGraph(graph.GetGraphElement(knoten,knoten)-MARKIERKOSTEN, knoten, knoten);
            return graph;
        }
        
        // Generieren einer ersten Bahnplanungslösung mit dem Nearest-Neighbor Verfahren
        private Druckfolge NearestNeighbor(Graph_ graph, int startNode)
        {                
            Druckfolge initialLösung = new Druckfolge();       
            // 1. Startpunkt = Erster Knoten
            int aktuellerKnoten = startNode;
            int minimumKnotenNummer = startNode;      
            MarkiereKnoten(aktuellerKnoten, graph);
            initialLösung.AddPriority((uint)startNode);
                 
            for (int i = 0; i < graph.GetGraph().Count - 1; i++)
            {                
                double minimum = MARKIERKOSTEN*10;
                for (int j = 0; j < graph.GetGraph().Count; j++)
                {
                    if (graph.GetGraphElement(aktuellerKnoten, j) < minimum)
                    {
                        minimum = graph.GetGraphElement(aktuellerKnoten, j);
                        minimumKnotenNummer = j;
                    }
                }
                // Füge den Knoten mit günstigster Kante in die Druckreihenfolge ein               
                initialLösung.AddPriority((uint) minimumKnotenNummer);
                initialLösung.SummiereGesamtkosten((uint) minimum);
                // Aktualisiere den Knoten von dem aus die günstigste Kante gesucht wird
                aktuellerKnoten = minimumKnotenNummer;                
                MarkiereKnoten(aktuellerKnoten, graph);
            }        
            return initialLösung;
        }

        public double CalculateDistanceAll(Druckfolge druckfolge, List<ushort[]> voxelList, bool isInfill)
        {
            double distanzKosten = 0;
            
            for (int i = 0; i < voxelList.Count; i++)
            {
                uint index = druckfolge.GetPriorityItem(i);
                Voxel v = new Voxel(voxelList[(int) index]);

                if ((i + 1) < voxelList.Count)
                {
                    uint index2 = druckfolge.GetPriorityItem((i + 1));
                    Voxel v2 = new Voxel(voxelList[(int) index2]);
                    if (v.IsNeighbor6(v2))
                        distanzKosten += EudklidDistanzAusVoxelDistanz(v.VoxelKoordinatenDistanz(v2));
                    else
                        distanzKosten += ABSETZKOSTEN + EudklidDistanzAusVoxelDistanz(v.VoxelKoordinatenDistanz(v2));
                }
            }                   
            return distanzKosten;
        }

        public void _2OptSwap(Druckfolge neueLösung, List<ushort[]> voxelList, bool isInfill, int i, int j)
        {
            Druckfolge swap = new Druckfolge(neueLösung.DeepCopy());
            
            for (int m = 0; m < i; m++)
            {
                neueLösung.SetPriority((int) swap.GetPriorityItem(m), m);
            }
            int decrease = 0;            
            for (int m = i; m <= j; m++)
            {
                    neueLösung.SetPriority((int)swap.GetPriorityItem(j-decrease),m);
                    decrease++;
            }
            for (int m = j + 1; m < neueLösung.GetPriority().Count; m++)
            {
                neueLösung.SetPriority((int)swap.GetPriorityItem(m),m);
            }
            neueLösung.SetGesamtkosten(CalculateDistanceAll(neueLösung, voxelList,isInfill));
        }
        
        public Druckfolge _2Opt(Druckfolge initialLösung, Graph_ graph, bool isInfill)
        {
            Druckfolge _2optLösung = new Druckfolge(initialLösung);
            Druckfolge neueLösung = _2optLösung.DeepCopy();
        
            /*
             * Instead of iterating through every possible value for i and j, only a few are taken as most of the time
             * more values don't yield significantly better results while the computing time is going up exponentially.
             */
            for (int i = 0; i < graph.GetVoxelKoordinaten().Count; i += graph.GetVoxelKoordinaten().Count / 10)
            {

                for (int j =  1; j < graph.GetVoxelKoordinaten().Count; j += graph.GetVoxelKoordinaten().Count / 40)
                {
                    neueLösung = _2optLösung.DeepCopy();
                    _2OptSwap(neueLösung,graph.GetVoxelKoordinaten(), isInfill, i, j);
                    if (!(neueLösung.GetGesamtkosten() > _2optLösung.GetGesamtkosten()))
                       _2optLösung = neueLösung.DeepCopy();
                }          
            }          
            return _2optLösung;
        }
        
        public Bahn Bahnplanung(List<Voxel> voxelList, int layerIndex)
        {
            /*
             * Teilen der gesamten Voxelliste in Randvoxel und Rest, damit unterschiedliche Bahnen geplant werden können
             * splitList[0] enthält Schichtränder
             * splitList[1] enthält alle anderen Voxel
             */            
            List<List<Voxel>> splitList = new List<List<Voxel>>(SplitVoxelList(voxelList));
            
            /*
             * Erstelle zwei Graphen : Randvoxel-Graph und Restvoxel-Graph
             * False und True zeigen hier jeweils nur an ob es sich bei den Verarbeitungsschritten um
             * Infillvoxel handelt oder nicht, wegen der Nachbarschaftskontrolle
             */
            Graph_ randGraph = new Graph_(VoxelToGraph(splitList[0], false));
            Graph_ restGraph = new Graph_(VoxelToGraph(splitList[1], true));

            // Erstellen der Druckfolgen
            Druckfolge initialRand = new Druckfolge();
            Druckfolge initialRest = new Druckfolge();

            Druckfolge _2optRand = new Druckfolge();
            Druckfolge _2optRest = new Druckfolge();
            
            Druckfolge optimizedRand = new Druckfolge(0.0);
            Druckfolge optimizedRest = new Druckfolge(0.0);

            for (int NNRUNS = 0; NNRUNS < 5; NNRUNS++)
            {
                Random randomizer = new Random();                
                int startNodeRand = (randomizer.Next(0, randGraph.GetGraph().Count-1));
                int startNodeRest = (randomizer.Next(0, restGraph.GetGraph().Count-1));
                // Generieren einer NN-Tour mit random Startknoten
                initialRand = NearestNeighbor(randGraph.DeepCopy(), startNodeRand);
                initialRest = NearestNeighbor(restGraph.DeepCopy(), startNodeRest);

                // Verbesserung der initialen Lösung durch 2-opt
                _2optRand = _2Opt(initialRand, randGraph.DeepCopy(), false);
                _2optRest = _2Opt(initialRest, restGraph.DeepCopy(), true);

                //Behalten des besten lokalen Optimums
                if (_2optRand.GetGesamtkosten() < optimizedRand.GetGesamtkosten())
                    optimizedRand = _2optRand.DeepCopy();
                if (_2optRest.GetGesamtkosten() < optimizedRest.GetGesamtkosten())
                    optimizedRest = _2optRest.DeepCopy();                
            }
            
            Bahn bahn = new Bahn(splitList, randGraph, restGraph, optimizedRand, optimizedRest, layerIndex);
            return bahn;
        }

        public void Textoutput(double robotGeschwindigkeit,
                               double extrusionsGeschwindigkeit,
                               string path,
                               string fileName)
        {
            /*
             * Textoutput für Koordinate(X,Y,Z), Orientierung(Winkel1,Winkel2,Winkel3), Robotergeschwindigkeit,
             * Extrusionsgeschwindigkeit vom Vorgänger zu diesem Punkt, Nummer der Schicht
             */

            List<bool> absetzPunkte = new List<bool>();
            List<double> absetzDouble = new List<double>();
            absetzPunkte.Add(false);
            int index = 0;
            int index2 = 0;
            using (StreamWriter outputFile = File.AppendText(path + fileName))
            {
                Voxel v = new Voxel(m_randGraph.GetVoxelKoordinatenAtIndex(0));
                Voxel v2;
                for (int i = 0; i < m_splitList[0].Count; i++)
                {
                    if (absetzPunkte[i])
                        //Hier Extrusionsgeschwindigkeit eintragen
                        absetzDouble.Add(extrusionsGeschwindigkeit);
                    else
                        absetzDouble.Add(0);
                    index = (int)m_optimizedRand.GetPriorityItem(i);                    
                    v = new Voxel(m_randGraph.GetVoxelKoordinatenAtIndex(index));
                    if( (i+1) < m_splitList[0].Count)
                        index2 = (int)m_optimizedRand.GetPriorityItem(i + 1);
                    if ((index2) < m_splitList[0].Count)
                    {
                        v2 = new Voxel(m_randGraph.GetVoxelKoordinatenAtIndex(index2));
                        absetzPunkte.Add(v.IsNeighbor6(v2));
                    } 
                    outputFile.Write(m_randGraph.GetVoxelKoordinate(0, index) + " " +
                                     m_randGraph.GetVoxelKoordinate(1, index) + " " +
                                     m_randGraph.GetVoxelKoordinate(2, index) + " " +
                                     m_splitList[0][index].getOrientierungAt(0) + " " +
                                     m_splitList[0][index].getOrientierungAt(1) + " " +
                                     m_splitList[0][index].getOrientierungAt(2) + " " +
                                     robotGeschwindigkeit + " " +
                                     absetzDouble[i] + " " + 
                                     m_layerIndex + " " +
                                     "\r\n");
                }
                //outputFile.Write("\r\n");
                absetzPunkte.Clear();
                absetzDouble.Clear();
                index = (int) m_optimizedRest.GetPriorityItem(0);
                v2 = new Voxel(m_restGraph.GetVoxelKoordinatenAtIndex(index));
                absetzPunkte.Add(v.IsNeighbor26(v2));

                for (int i = 0; i < m_splitList[1].Count; i++)
                {
                    if (absetzPunkte[i])
                        //Hier Extrusionsgeschwindigkeit eintragen
                        absetzDouble.Add(extrusionsGeschwindigkeit);
                    else
                        absetzDouble.Add(0);
                    index = (int)m_optimizedRest.GetPriorityItem(i);
                    v = new Voxel(m_restGraph.GetVoxelKoordinatenAtIndex(index));
                    if( (i+1) < m_splitList[1].Count)
                        index2 = (int)m_optimizedRest.GetPriorityItem(i + 1);
                    if ((index2) < m_splitList[1].Count)
                    {
                        v2 = new Voxel(m_restGraph.GetVoxelKoordinatenAtIndex(index2));
                        absetzPunkte.Add(v.IsNeighbor26(v2));
                    } 
                    outputFile.Write(m_restGraph.GetVoxelKoordinate(0, index) + " " +
                                     m_restGraph.GetVoxelKoordinate(1, index) + " " +
                                     m_restGraph.GetVoxelKoordinate(2, index) + " " +
                                     m_splitList[1][index].getOrientierungAt(0) + " " +
                                     m_splitList[1][index].getOrientierungAt(1) + " " +
                                     m_splitList[1][index].getOrientierungAt(2) + " " +
                                     robotGeschwindigkeit + " " +
                                     absetzDouble[i] + " " + 
                                     m_layerIndex + " " +
                                     "\r\n");
                }
            }   
        }            
    }  
}