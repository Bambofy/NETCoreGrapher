using System;
using System.Linq;
using System.Numerics;
using Flee;
using Flee.PublicTypes;

namespace ConsoleGrapher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;


            ExpressionContext context = new ExpressionContext();
            context.Imports.AddType((typeof(Math)));

            Console.WriteLine("Please enter an expression (only variable avilable is `x`):");
            string expression = Console.ReadLine();
            expression = expression.ToLowerInvariant();

            foreach (char constChar in expression.Where((char c) =>
            {
                if (c == 'x') return false;

                return Char.IsLetter(c);
            }))
            {
                Console.WriteLine("Enter the value for the constant `{0}`", constChar);
                String constValue = Console.ReadLine();
                context.Variables[Convert.ToString(constChar)] = Convert.ToDouble(constValue);
            }



            Console.WriteLine("Please enter the bounds for the graph...");
            Console.Write("X Min: ");
            String xMinStr = Console.ReadLine();
            Console.Write("X Max: ");
            String xMaxStr = Console.ReadLine();
            Console.Write("Y Min: ");
            String yMinStr = Console.ReadLine();
            Console.Write("Y Max: ");
            String yMaxStr = Console.ReadLine();

            Console.WriteLine("Please enter the interval for the labels (1.0 being every cell labeled, 0.0 being no labels, default is 0.5)");
            Console.Write("X Label Interval (0.5): ");
            String xLabelStr = Console.ReadLine();
            Console.Write("Y Label Interval (0.5): ");
            String yLabelStr = Console.ReadLine();



            double xLabelInterval = Convert.ToDouble(xLabelStr);
            double yLabelInterval = Convert.ToDouble(yLabelStr);

            // Limits for the graph.
            double xMin = Convert.ToDouble(xMinStr);
            double xMax = Convert.ToDouble(xMaxStr);

            double yMin = Convert.ToDouble(yMinStr);
            double yMax = Convert.ToDouble(yMaxStr);

            // how many "pixels" do we have to use?
            int consoleWidth = Console.WindowWidth;
            int consoleHeight = Console.WindowHeight;


            // top left
            // when C = (0,0) the cartesian grid will be at (xMin, yMax)

            // top right
            // when C = (consoleWidth, 0) the grid will be at (xMax, yMax)

            // bottom left
            // when C = (0, consoleHeight) the grid will be at (xMin, yMin)

            // bottom right
            // when C = (consoleWidth, consoleHeight) the grid will be at (xMax, yMin)


            // find the constant offset 'c' here...
            // C(0, consoleHeight) = G(xMin, yMin)
            // when cY = consoleHeight, gY = yMin




            // VERTICAL LINE (y values) //
            // cY = 0, gY = yMax
            // cY = 120, gY = yMin

            double consoleYRange = consoleHeight;
            double graphYRange = yMax - yMin;

            double YRangeScale = (graphYRange / consoleYRange);

            // double gY = yMax - (cY * YRangeScale);

            // HORIZONTAL LINE (x values) //
            // cX = 0, gX = xMin
            // cX = 120, gX = xMax
            double consoleXRange = consoleWidth;
            double graphXRange = xMax - xMin;

            double XRangeScale = (graphXRange / consoleXRange);

            //double gX = xMin + (XRangeScale * cX)


            //for (int cX = 0; cX < consoleWidth; cX++)
            //{
            //    double gX = xMin + (cX * XRangeScale);
            //    Console.WriteLine(gX);
            //}
            //Console.ReadLine();


            //for (int cY = 0; cY < consoleHeight; cY++)
            //{
            //    double gY = yMax - (cY * YRangeScale);
            //    Console.WriteLine(gY);
            //}
            //Console.ReadLine();

            int xLabelOffset = (int)((double)xLabelInterval * ((double)consoleWidth / 2.0));
            int yLabelOffset = (int)((double)yLabelInterval * ((double)consoleHeight / 2.0));

            Console.SetCursorPosition(0, 0);

            // Draw background.
            for (int xC = 0; xC < consoleWidth; xC++)
            {
                for (int yC = 0; yC < consoleHeight; yC++)
                {
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write(' ');
                }
            }

            // Draw Graph axis
            int xLabelCounter = 0;
            int yLabelCounter = 0;
            for (int xC = 0; xC < consoleWidth; xC++)
            {
                for (int yC = 0; yC < consoleHeight; yC++)
                {
                    Console.SetCursorPosition(xC, yC);

                    double gX = xMin + (xC * XRangeScale);
                    double gY = yMax - (yC * YRangeScale);

                    // x axis
                    bool xAxis = yC == (consoleHeight / 2);
                    bool yAxis = xC == (consoleWidth / 2);

                    if (xAxis && yAxis)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write('+');
                    }
                    else if (xAxis)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write('-');

                        xLabelCounter++;
                        if (xLabelCounter >= xLabelOffset)
                        {
                            Console.SetCursorPosition(xC, yC + 1);
                            Console.Write("{0:0.00}", gX);
                            xLabelCounter = 0;
                        }

                    }
                    else if (yAxis)
                    {
                        Console.BackgroundColor = ConsoleColor.White;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write('|');

                        yLabelCounter++;
                        if (yLabelCounter >= yLabelOffset)
                        {
                            Console.SetCursorPosition(xC - 1, yC);
                            Console.Write("{0:0.00}", gY);
                            yLabelCounter = 0;
                        }
                    }

                }
            }



            // plot graph.
            for (int xC = 0; xC < consoleWidth; xC++)
            {
                // find closest point to the yExpressed.
                double closestGYDifference = double.PositiveInfinity;
                int closestCY = -1;

                // calculate the correct value at this X position.
                double gX = xMin + (xC * XRangeScale);
                context.Variables["x"] = gX; // varies

                IGenericExpression<double> genericExpression = context.CompileGeneric<double>(expression);
                double yCalculated = genericExpression.Evaluate();

                for (int yC = 0; yC < consoleHeight; yC++)
                {
                    // grid values at this character position.
                    double gY = yMax - (yC * YRangeScale);

                    double gYDifference = Math.Sqrt(Math.Pow(yCalculated - gY, 2));
                    if (gYDifference < closestGYDifference)
                    {
                        closestGYDifference = gYDifference;
                        closestCY = yC;
                    }
                }

                // plot the character at the closest character Y value.
                if (closestCY != -1)
                {
                    Console.SetCursorPosition(xC, closestCY);
                    Console.BackgroundColor = ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write('@');
                }
            }

            Console.ReadLine();


        }
    }


}
