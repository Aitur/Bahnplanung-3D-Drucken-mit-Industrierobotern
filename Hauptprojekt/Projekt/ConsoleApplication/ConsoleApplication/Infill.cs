using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Werkzeugbahnplanung
{
    class Infill
    {
        private int[,,] infill_baseCell;
        private int infill_density;
        private int infill_offset;
        private string infill_type;

        public Infill(int density, string type, int offset = 0) {
            if (density != 0)
            {
                infill_density = (100 / density + 1) / 2;
                infill_density *= 2;
                infill_type = type;
                infill_offset = offset;
                if (type == "3DInfill")
                {
                    Console.WriteLine("Preparing 3DInfill");
                    infill_baseCell = Generate_3DInfill();
                    Console.WriteLine("Finished preparing 3DInfill");
                }
                else if (type == "HexInfill")
                {
                    Console.WriteLine("Preparing HexInfill");
                    infill_baseCell = Generate_HexInfill();
                    Console.WriteLine("Finished preparing HexInfill");
                }
            }
            else {
                infill_type = "Empty";
            }

        }

        public int IsInfill(int x, int y=0, int z=0) {
            /*prone to errors do not change this and keep return value and parameters for all Is_ Methodes equal
            Object[] param = new Object[] { x, y, z }; significant(10%) slowdown.
            return Int32.Parse(typeof(Infill).GetMethod(infill_type).Invoke(this, param).ToString());
            */
            switch (infill_type)
            {
                case "3DInfill":
                    return Is_3DInfill(x, y, z);
                case "HexInfill":
                    return Is_HexInfill(x, y, z);
                case "Empty":
                    return Is_Empty(x, y, z);
                case "LineInfill":
                    return Is_LineInfill(x, y, z);
                case "Line3DInfill":
                    return Is_Line3DInfill(x, y, z);
                default:
                    //Non valid Infill Type
                    return 0;
            }
        }

        public int[,,] Generate_3DInfill()
        {
            int[,,] Sample = new int[2 * infill_density, 2*infill_density, 2 * infill_density];
            //Definition der Einzel Zelle
            int density_half = infill_density / 2;
            //Octel
            for (int height = 0; height < density_half; height++)//lower half
            {
                for (int i = 0; i < density_half-height; i++)
                {
                    Sample[i, infill_density, height] = 1;//width
                    Sample[infill_density, i, height] = 1;//depth
                }
                for (int i = 0; i <= density_half + height; i++)
                {
                    Sample[density_half + i - height, infill_density - i, height] = 1;//diagonal
                }
            }

            for (int height = 0; height <= density_half; height++)//upper half
            {
                for (int i = 0; i < density_half-height; i++)
                {
                    Sample[0, infill_density - i, infill_density - height] = 1;//width
                    Sample[infill_density - i, 0, infill_density - height] = 1;//depth
                }
                for (int i = 0; i <= infill_density - height; i++)
                {
                    Sample[infill_density - height - i, i, density_half + height] = 1;//diagonal
                }
            }

            for (int i = 0; i <= density_half; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    Sample[density_half + i, infill_density - j, 0] = 1;//flat bottom surface
                    Sample[density_half-i, j, infill_density] = 1;//flat top surface
                }

            }
            //mirror alongside the x-axis
            for (int width = 0; width < infill_density; width++)
            {
                for (int depth = 0;depth <= infill_density ;depth++) {
                    for (int height = 0; height <= infill_density; height++)
                    {
                        Sample[infill_density+width,depth,height]= Sample[infill_density -width,depth,height];
                    }
                }
            }
            //mirror alongsite the y-axis
            for (int width = 0; width < infill_density*2; width++)
            {
                for (int depth = 0; depth < infill_density; depth++)
                {
                    for (int height = 0; height <= infill_density; height++)
                    {
                        Sample[width, infill_density + depth, height] = Sample[width, infill_density - depth, height];
                    }
                }
            }
            //mirror alogsite the z-axis
            for (int width = 0; width < infill_density * 2; width++)
            {
                for (int depth = 0; depth < infill_density * 2; depth++)
                {
                    for (int height = 0; height < infill_density; height++)
                    {
                        Sample[width, depth, infill_density + height] = Sample[width, depth, infill_density - height];
                    }
                }
            }
            return Sample;
        }

        public int Is_3DInfill(int x, int y, int z)
        {
            y = y % (2*infill_density);
            x = x % (2*infill_density);
            z = z % (2*infill_density);
            return infill_baseCell[x,y,z];
        }
        

        public int[,,] Generate_HexInfill()
        {
            int half_density = infill_density / 2;
            int[,,] Sample = new int[(infill_density+half_density), 2*infill_density , 1];
            //Definition der Einzel Zelle
            for (int width = 0; width < infill_density; width++)
            {
                Sample[half_density + width, 0, 0] = 1;
            }
            for (int width = 0; width < half_density; width++)
            {
                Sample[width, (half_density-width - 1) * 2, 0] = 1;
                Sample[width, (half_density - width - 1) * 2 + 1 , 0] = 1;
                Sample[width, 2*infill_density-((half_density - width - 1) * 2 + 1), 0] = 1;
                Sample[width, 2 * infill_density - ((half_density - width - 1) * 2 + 2), 0] = 1;
            }
            return Sample;
        }

        public int Is_HexInfill(int x, int y, int z)
        {
            Boolean isEven = (0 == (x / (infill_density + (infill_density / 2) - 1)) % 2);
            x = x % (infill_density + (infill_density / 2) - 1);
            y = y % (2 * infill_density - 1);
            if (!isEven)
            {
                y += (infill_density - 1);
                y = y % (2 * infill_density - 1);
            }
            return infill_baseCell[x, y, 0];
        }
        public int Is_LineInfill(int x, int y, int z)
        {
            if (x % (infill_density * 2) == infill_offset || y % (infill_density * 2) == infill_offset) {
                return 1;
            }
            return 0;
        }

        public int Is_Line3DInfill(int x, int y, int z)
        {
            //figure out how too turn offset in ° -> 45° tilt:
            if (infill_offset != 0) {
                if (0 == (z+y+(infill_density*4-x)) % (infill_density * 4)) {
                    return 1;
                }
                int plane0011 = z + y + x;
                int plane0001 = z + x + (infill_density * 4 - y);
                x = y + x +(infill_density * 4 - z);
                y = plane0011;
                z = plane0001;
                
            }
            if (x % (infill_density * 4) == 0 || y % (infill_density * 4) == 0 || z % (infill_density * 4) == 0)
            {
                return 1;
            }
            return 0;
        }
        public int Is_Empty(int x, int y, int z)
        {
            return 0;
        }
    }
}
