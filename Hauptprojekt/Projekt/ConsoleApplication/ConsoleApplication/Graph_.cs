using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Werkzeugbahnplanung
{
/*
 * Jeder Graph besteht aus zwei Teilen.
 * Die Graphliste, die die Kosten jeden Pfades von Knoten i zu Knoten j und von Knoten j zu Knoten i speichert.
 * (Speicherverbrauch könnte effektiv halbiert werden)
 * Und den Voxelkoordinaten, die die Knoten im Graph repräsentieren.
 */
    public class Graph_
    {

        private List<List<double>> m_graph;
        private List<ushort[]> m_VoxelKoordinaten;
        
        //Konstruktoren
        public Graph_()
        {
            m_graph = new List<List<double>>();
            m_VoxelKoordinaten = new List<ushort[]>();
        }
 
        public Graph_(List<List<double>> graph, List<ushort[]> voxelKoordinaten)
        {
            m_graph = new List<List<double>>(graph);
            m_VoxelKoordinaten = new List<ushort[]>(voxelKoordinaten);
        }

        public Graph_(Graph_ graph)
        {
            m_graph = graph.m_graph;
            m_VoxelKoordinaten = graph.m_VoxelKoordinaten;
        }

        //DeepCopy eines Graphen
        public Graph_ DeepCopy()
        {    
            Graph_ deepCopy = new Graph_();
            deepCopy.m_graph = new List<List<double>>(GetGraph());
            deepCopy.m_VoxelKoordinaten = new List<ushort[]>(m_VoxelKoordinaten);
            return deepCopy;
        }

        //Getter
        public List<List<double>> GetGraph()
        {
            return m_graph;
        }

        public double GetGraphElement(int i, int j)
        {
            return m_graph[i][j];
        }

        public List<ushort[]> GetVoxelKoordinaten()
        {
            return m_VoxelKoordinaten;
        }

        public ushort[] GetVoxelKoordinatenAtIndex(int i)
        {
            return m_VoxelKoordinaten[i];
        }

        public ushort GetVoxelKoordinate(ushort koor, int i)
        {
            /*
             *  index == 0 -> x-Koordinate
             *  index == 1 -> y-Koordinate
             *  index == 2 -> z-Koordinate
             */
            ushort[] koordinaten = m_VoxelKoordinaten[i];
            return koordinaten[koor];
        }

        //Setter
        public void SetGraph(List<List<double>> graph)
        {
            m_graph = graph;
        }

        public void SetGraph(double d, int i, int j)
        {
            m_graph[i][j] = d;
        }

        public void SetVoxelKoordinaten(List<ushort[]> voxelKoordinaten)
        {
            m_VoxelKoordinaten = voxelKoordinaten;
        }
        
        public void SetVoxelKoordinatenAtIndex(ushort[] k ,int i)
        {
            m_VoxelKoordinaten[i] = k;
        }
        
        //Add (für leere Listen)
        public void AddGraphElement(List<double> d)
        {
            m_graph.Add(d);
        }

        public void AddVoxelKoordinaten(ushort[] k)
        {
            m_VoxelKoordinaten.Add(k);
        }
    }
}