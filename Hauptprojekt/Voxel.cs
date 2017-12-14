﻿using System;

namespace Werkzeugbahnplanung
{
   public class Voxel
    {
        private bool m_Schichtrand;
        private bool m_Modellrand;
        private ushort[] m_koordinaten;

        public Voxel()
        {
            m_Schichtrand = false;
            m_Modellrand = false;
            m_koordinaten = new ushort[3]{0,0,0};
        }

        public Voxel(bool schichtrand, bool modellrand, ushort xKoord, ushort yKoord, ushort zKoord)
        {
            m_Schichtrand = schichtrand;
            m_Modellrand = modellrand;
            m_koordinaten = new ushort[3];
            m_koordinaten[0] = xKoord;
            m_koordinaten[1] = yKoord;
            m_koordinaten[2] = zKoord;
        }

        public Voxel(ushort[] koordinaten)
        {
            m_Schichtrand = false;
            m_Modellrand = false;
            m_koordinaten = koordinaten;
        }

        public void setSchichtrand(bool value)
        {
            m_Schichtrand = value;
        }

        public bool getSchichtrand()
        {
            return m_Schichtrand;
        }

        public void setModellrand(bool value)
        {
            m_Modellrand = value;
        }

        public bool getModellrand()
        {
            return m_Modellrand;
        }

        public ushort[] getKoords()
        {
            return m_koordinaten;
        }
        
        //Prüfe ob zwei Voxel benachbart sind
        public bool IsNeighbor26(Voxel a)
        {
            int[] distanz = this.VoxelKoordinatenDistanz(a);
            int distanzSumme = distanz[0] + distanz[1] + distanz[2];
            if ((distanz[0] <= 1 && distanz[1] <= 1 && distanz[2] <= 1)&&(distanzSumme != 0))
                return true;
            else
                return false;
        }

        public bool IsNeighbor6(Voxel a)
        {
            int[] distanz = this.VoxelKoordinatenDistanz(a);
            if ((distanz[0] == 0 && distanz[1] == 0 && distanz[2] == 1) ||
                (distanz[0] == 0 && distanz[1] == 1 && distanz[2] == 0) ||
                (distanz[0] == 1 && distanz[1] == 0 && distanz[2] == 0))
                return true;
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
