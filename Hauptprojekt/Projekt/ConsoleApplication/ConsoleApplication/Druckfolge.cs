using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Werkzeugbahnplanung
{
    /*
     * Jede Druckfolge besteht aus einer Prioritätsliste die die Indices von Voxeln speichert,
     * sodass diese in einer bestimmten Reihenfolge abgearbeitet werden können. Außerdem werden die
     * Gesamtkosten gespeichert die sich durch diesen bestimmten Weg ergeben würden. Die Kosten sind
     * ein absolutes Maß im Sinne der gefahrenen Distanz. Die Kosten können aber auch verzerrt werden durch die
     * ABSETZKOSTEN und MARKIERKOSTEN.
     */
    public class Druckfolge
    {
        private List<uint> m_priority;
        private double m_gesamtKosten;

        //Konstruktoren
        public Druckfolge()
        {
            m_priority = new List<uint>();
            m_gesamtKosten = 0;
        }

        public Druckfolge(double GesamtKosten)
        {
            m_priority = new List<uint>();
            m_gesamtKosten = Double.MaxValue;
        }

        public Druckfolge(Druckfolge druckfolge)
        {
            m_priority = new List<uint>(druckfolge.m_priority);
            m_gesamtKosten = druckfolge.m_gesamtKosten;
        }
        
        //DeepCopy eines Graphen
        public Druckfolge DeepCopy()
        {      
            Druckfolge deepCopy = new Druckfolge();
            deepCopy.m_priority = new List<uint>(GetPriority());
            deepCopy.m_gesamtKosten = GetGesamtkosten();
            return deepCopy; 
        }

        
        //Getter
        public List<uint> GetPriority()
        {
            return m_priority;
        }

        public uint GetPriorityItem(int i)
        {
            return m_priority[i];
        }

        public double GetGesamtkosten()
        {
            return m_gesamtKosten;
        }
            
        //Setter
        public void SetPriority(List<uint> priority)
        {
            m_priority = priority;
        }
        
        public void SetPriority(int u, int i)
        {
            m_priority[i] = (uint)u;
        }

        public void SetGesamtkosten(double u)
        {
            m_gesamtKosten = u;
        }
        
        //Add (für leere Listen)
        public void AddPriority(uint u)
        {
            m_priority.Add(u);
        }

        //Sonstiges
        public void SummiereGesamtkosten(uint u)
        {
            m_gesamtKosten += u;
        }
    }
}