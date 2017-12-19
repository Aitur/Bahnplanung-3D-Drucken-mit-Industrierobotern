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
                infill_baseCell = Generate_3DInfill();
                infill_type = "Is_" + type;
                infill_offset = offset;
                if (type == "3DInfill")
                {
                    Generate_3DInfill();
                }
                else if (type == "HexInfill")
                {
                    Generate_HexInfill();
                }
            }
            else {
                infill_type = "Is_Empty";
            }

        }

        public int IsInfill(int x, int y=0, int z=0) {
            //prone to errors do not change this and keep return value and parameters for Is_ Methodes equal
            Object[] param = new Object[] { x, y, z };
            return Int32.Parse(typeof(Infill).GetMethod(infill_type).Invoke(this, param).ToString());

        }

        public int[,,] Generate_3DInfill()
        {
            int[,,] Sample = new int[2 * infill_density, infill_density, 2 * infill_density];
            //Definition der Einzel Zelle
            //Bottom Half
            for (int depth = 0; depth <= infill_density / 2; ++depth)
            {
                for (int width = infill_density / 2 + depth; width < infill_density + infill_density / 2 - depth - 1; ++width)
                {
                    Sample[width, depth, 0] = 1;//Bottom flat triangle
                }
            }
            for (int height = 0; height < infill_density / 2; ++height)
            {
                for (int depth = 0; depth <= infill_density / 2 - height; ++depth)
                {
                    Sample[infill_density - 1, infill_density / 2 + depth + height-1, height] = 1;//Bottom standing triangle 
                    Sample[infill_density*2-1, depth, infill_density - height] = 1;//right side standing triangle 
                }
                
                for (int depth = 0; depth < infill_density / 2 + height; ++depth)//faces
                {
                    Sample[infill_density / 2 - height + depth, depth, height ] = 1;
                    Sample[infill_density + infill_density / 2 + height - 1 - depth-1, depth, height] = 1;//lower half

                    Sample[infill_density / 2 + height - depth - 1, infill_density - depth - 2, infill_density - height-1] = 1;
                    Sample[infill_density + infill_density / 2 - height + depth-1, infill_density - depth-2, infill_density - height-1] = 1;//upper half
                }
            }
            for (int height = 0; height <= infill_density / 2; ++height)
            {
                for (int width = infill_density-height-1; width <= infill_density + height; ++width)
                {
                    Sample[width, infill_density - 2, infill_density / 2 + height] = 1;//line
                }
            }
            for (int width = 0; width < infill_density / 2; ++width)
            {
                for (int depth = 0; depth < infill_density / 2 - width; ++depth)
                {
                    Sample[width, infill_density - 1 - depth, infill_density] = 1;//Mid flat Triangle
                    Sample[2 * infill_density - 1 - width, infill_density - 2 - depth, infill_density] = 1;
                }
            }
            //return Sample;
            for (int i = 0; i < infill_density; i++)//mirror the lower half to the upper half
            {
                for (int y = 0; y < infill_density; ++y)
                {
                    for (int x = 0; x < 2*infill_density; ++x)
                    {
                        Sample[x, y, 2 * infill_density - 1 - i] = Sample[x, y, i + 1];
                    }
                }
            }
            return Sample;
        }

        public int Is_3DInfill(int x, int y, int z)
        {
            Boolean isEven = (0 == (y/(infill_density-1))% 2);
            y = y % (infill_density-1);
            x = x % (2*infill_density);
            z = z % (2*infill_density);
            if (!isEven)
            {
                x += (infill_density);
                z += (infill_density);
                x = x % (2 * infill_density);
                z = z % (2 * infill_density);
            }
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
            y = y % (2 * infill_density - 1);
            x = x % (infill_density + (infill_density / 2) - 1);
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
