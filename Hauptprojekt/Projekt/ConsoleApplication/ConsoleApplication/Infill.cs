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
        //baseCell is a non Mandatory variable meant to represent a simple pattern which will be repeated to fill the voxelmodell
        private int[,,] infill_baseCell;
        private int infill_density;
        private int infill_offset;
        private string infill_type;

        public Infill(int density, string type, int offset = 0)
        {
            if (density != 0)//if there are no Voxel meant to be filled set the type to empty, to avoid unneccesary computation
            {
                //the infills density is fit to the corresponding functions, which is slightly different for 3DInfill as it has to be round up in case it is uneven
                infill_density = 100 / density;
                infill_type = type;
                infill_offset = offset;
                if (type == "3DInfill")//in case the infill is created using tesselation 2 or 3Dimensional, the pattern will be generated here
                {
                    infill_density = (infill_density + 1) / 2;
                    infill_density *= 2;
                    Console.WriteLine("Preparing 3DInfill");
                    infill_baseCell = Generate_3DInfill();
                    Console.WriteLine("Finished preparing 3DInfill");
                }
                else if (type == "HexInfill")
                {
                    infill_density = (infill_density + 1) / 2;
                    infill_density *= 2;
                    Console.WriteLine("Preparing HexInfill");
                    infill_baseCell = Generate_HexInfill();
                    Console.WriteLine("Finished preparing HexInfill");
                }
            }
            else
            {
                infill_type = "Empty";
            }

        }

        /*
        This Method is used for all types of infill, this 'interface' is used because overwriting the function using reflection:
        '
            Object[] param = new Object[] { x, y, z }; 
            Int32.Parse(typeof(Infill).GetMethod(infill_type).Invoke(this, param).ToString());
        '
            yields a significant(10%) slowdown.
        The use of optional parameters only serves to add flexibility
        The Method defaults to 0 which is equal to the result of Is_Empty.    
        */
        public int IsInfill(int x, int y = 0, int z = 0)
        {
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

        //This Method depends on density,
        //and is used to generate a sample Truncated octahedron with attatchments to properly execute Is_3DInfill.
        public int[,,] Generate_3DInfill()
        {
            //The Truncated octahedron with attachments would be 2times+1 the density in all dimensions in size 
            //but to avoid overlapping the samples will be repeated beginning with their intersections, 
            //which (the intersections) therefore do not have to be part of the sample.
            int[,,] Sample = new int[2 * infill_density, 2 * infill_density, 2 * infill_density];
            int density_half = infill_density / 2;

            //The following paragraph will create a bottom corner of the Sample in two parts as well as top and bottom.
            //removing one of the lines inside the loops will result in one face or attatchment to be removed 
            for (int height = 0; height < density_half; height++)//lower part
            {
                for (int i = 0; i < density_half - height; i++)
                {
                    Sample[i, infill_density, height] = 1;//width
                    Sample[infill_density, i, height] = 1;//depth
                }
                for (int i = 0; i <= density_half + height; i++)
                {
                    Sample[density_half + i - height, infill_density - i, height] = 1;//diagonal
                }
            }

            for (int height = 0; height <= density_half; height++)//upper part
            {
                for (int i = 0; i < density_half - height; i++)
                {
                    Sample[0, infill_density - i, infill_density - height] = 1;//width
                    Sample[infill_density - i, 0, infill_density - height] = 1;//depth
                }
                for (int i = 0; i <= infill_density - height; i++)
                {
                    Sample[infill_density - height - i, i, density_half + height] = 1;//diagonal
                }
            }

            for (int i = 0; i <= density_half; i++)//top and bottom
            {
                for (int j = 0; j < i; j++)
                {
                    Sample[density_half + i, infill_density - j, 0] = 1;//flat bottom surface
                    Sample[density_half - i, j, infill_density] = 1;//flat top surface
                }

            }

            //In this paragraph the previously generated corner will be mirrored to create the symmetrical Truncated octahedron
            //mirror alongside the x-axis
            for (int width = 0; width < infill_density; width++)//This is deliberately inaccurate to realise leaving out the intersections between samples
            {
                for (int depth = 0; depth <= infill_density; depth++)
                {
                    for (int height = 0; height <= infill_density; height++)
                    {
                        Sample[infill_density + width, depth, height] = Sample[infill_density - width, depth, height];
                    }
                }
            }
            //mirror everything alongsite the y-axis
            for (int width = 0; width < infill_density * 2; width++)
            {
                for (int depth = 0; depth < infill_density; depth++)//deliberately inaccurate
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
                    for (int height = 0; height < infill_density; height++)//deliberately inaccurate
                    {
                        Sample[width, depth, infill_density + height] = Sample[width, depth, infill_density - height];
                    }
                }
            }
            return Sample;
        }

        //This Method depends on the base and density, it calculates whether the base would be filled for a voxel with the coordinates y, x, z
        //It is fit to work with a base created using Generate_3DInfill
        public int Is_3DInfill(int x, int y, int z)
        {
            y = y % (2 * infill_density);
            x = x % (2 * infill_density);
            z = z % (2 * infill_density);
            return infill_baseCell[x, y, z];
        }

        //This Method depends on density,
        //and is used to generate a sample half hexagon to properly execute Is_HexInfill.
        //It works with a similar approach to Generate_3DInfill
        public int[,,] Generate_HexInfill()
        {

            int half_density = infill_density / 2;
            int[,,] Sample = new int[(infill_density + half_density), 2 * infill_density, 1];

            for (int width = 0; width < infill_density; width++)
            {
                Sample[half_density + width, 0, 0] = 1;
            }

            for (int width = 0; width < half_density; width++)
            {
                Sample[width, (half_density - width - 1) * 2, 0] = 1;
                Sample[width, (half_density - width - 1) * 2 + 1, 0] = 1;
                Sample[width, 2 * infill_density - ((half_density - width - 1) * 2 + 1), 0] = 1;
                Sample[width, 2 * infill_density - ((half_density - width - 1) * 2 + 2), 0] = 1;
            }

            return Sample;
        }

        //This Method depends on the base and density and reacts to offset with a shift every second layer in height,
        //it calculates whether the base would be filled for a voxel with the coordinates y, x, z
        //for the base generated by Generate_HexInfill this tesselation requires a shift in its repetition alongsite the x-axis
        public int Is_HexInfill(int x, int y, int z)
        {
            if(z % 2 == 1)
            {
                x = x + infill_offset;
            }
            Boolean isEven = (0 == (x / (infill_density + (infill_density / 2) - 1)) % 2);
            x = x % (infill_density + (infill_density / 2) - 1);
            y = y % (2 * infill_density - 1);
            if (!isEven)
            {
                y += infill_density - 1;
                y = y % (2 * infill_density - 1);
            }
            return infill_baseCell[x, y, 0];
        }

        //This Method depends on the density and reacts to offsetand reacts to offset with a shift every second layer in height,
        //it calculates if a voxel with the coordinates y, x, z would be part of straight lines filling the Model axis aligned
        public int Is_LineInfill(int x, int y, int z)
        {
            if (z % 2 == 1)
            {
                x = x + infill_offset;
                y = y + infill_offset;
            }
            if (x % (infill_density * 2) == infill_offset || y % (infill_density * 2) == infill_offset)
            {
                return 1;
            }
            return 0;
        }

        //This Method depends on the density, it calculates if a voxel with the coordinates y, x, z would be part of infill consisting of stacked cubes.
        //If Offset is not 0 an experimental 3D Infill will be used.
        public int Is_Line3DInfill(int x, int y, int z)
        {
            if (infill_offset != 0)
            {
                if (0 == (z + y + (infill_density * 4 - x)) % (infill_density * 4))
                {
                    return 1;
                }
                int plane0011 = z + y + x;
                int plane0001 = z + x + (infill_density * 4 - y);
                x = y + x + (infill_density * 4 - z);
                y = plane0011;
                z = plane0001;

            }
            if (x % (infill_density * 4) == 0 || y % (infill_density * 4) == 0 || z % (infill_density * 4) == 0)
            {
                return 1;
            }
            return 0;
        }

        //This Method will lead to the creation of no infill 
        public int Is_Empty(int x, int y, int z)
        {
            return 0;
        }
    }
}
