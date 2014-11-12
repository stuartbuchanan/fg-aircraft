﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.DataVisualization.Charting;
using System.Web.UI.WebControls;


namespace F14AeroPlot
{
    public partial class JSBsim : System.Web.UI.Page
    {
        AeroReader aero = new AeroReader();
        protected void Page_Load(object sender, EventArgs e)
        {

            HtmlTextWriter writer = new HtmlTextWriter(Page.Response.Output);
//            writer.Write("<h1>F14 Aerodynamics</h1>");
            int w = 400;
            int h = 600;
            writer.Write("<?xml version=\"1.0\"?>");
            writer.Write("<?xml-stylesheet type=\"text/xsl\" href=\"http://jsbsim.sourceforge.net/JSBSim.xsl\"?>");
            writer.Write("<pre>\n");
            foreach (var aero_axis in aero.GetElements().GroupBy(xx => GetAxis(xx)))
            {
                var coeffs = new List<String>();
                foreach (var aero_element in aero_axis)
                {
//                    if (aero.HasData(aero_element))
                    {
                        var aerodat_item = GetVar(aero_element);

                        if (!aero.IsUsed(aero_element))
                            writer.Write("    <!--\n");
                        else
                            coeffs.Add(aerodat_item);

                        if (aero.Is2d(aero_element))
                        {
                            //                          writer.Write("<h2>{0}</h2>", aero_element);
                            //                          writer.Write("<pre>\n");

                            writer.Write("    <function name=\"aero/coefficients/{0}\">\n", aerodat_item);
                            writer.Write("    <description>{0}</description>\n", aero.GetDescription(aerodat_item));
                            writer.Write("    <product>\n");
                            OutputExtraIndependentVariables(writer, aero_element);
                            writer.Write("          <table>\n");
                            writer.Write("            <independentVar lookup=\"row\">aero/alpha-deg</independentVar>\n");
                            writer.Write("            <independentVar lookup=\"column\">aero/beta-deg</independentVar>\n");
                            writer.Write("            <tableData>");
                            var leading = "                 ";
                            var aero_data_element = aero.GetValues_2dAlphaBeta(aero_element);
                            writer.Write("\n" + leading);
                            writer.Write("       ");
                            foreach (var beta in aero_data_element.First().Value)
                            {
                                writer.Write("{0,10} ", beta.Key);
                            }

                            foreach (var alpha in aero_data_element)
                            {
                                writer.Write("\n" + leading);
                                writer.Write("{0,-6} ", alpha.Key);
                                foreach (var vv in alpha.Value)
                                {
                                    var q = vv.Value;
                                    writer.Write("{0,10:0.00000000} ", vv.Value);
                                }
                            }
                            writer.Write("\n            </tableData>\n");
                            writer.Write("          </table>\n");
                            writer.Write("       </product>\n");
                            writer.Write("    </function>\n");

                            //                      writer.Write("<pre>\n");
                        }
                        else
                        {

                            var aero_data_element = aero.GetValues_1d(aero_element);
                            if (aero_data_element != null)
                            {
                                //                        writer.Write("<h2>{0}</h2>", aero_element);
                                //                        writer.Write("<pre>\n");

                                writer.Write("    <function name=\"aero/coefficients/{0}\">\n", aerodat_item);
                                writer.Write("    <description>{0}</description>\n", aero.GetDescription(aerodat_item));
                                writer.Write("    <product>\n");
                                OutputExtraIndependentVariables(writer, aero_element);
                                writer.Write("          <table>\n");
                                writer.Write("            <independentVar lookup=\"row\">aero/alpha-deg</independentVar>\n");
                                writer.Write("            <tableData>\n");
                                var leading = "                 ";
                                foreach (var q in aero_data_element)
                                {
                                    writer.Write("{0}{1,6} {2,10:0.00000000}\n", leading, q.Key, q.Value);
                                }
                                writer.Write("            </tableData>\n");
                                writer.Write("          </table>\n");
                                writer.Write("       </product>\n");
                                writer.Write("    </function>\n");
                                //                        writer.Write("<pre>\n");
                            }
                        }
                        if (!aero.IsUsed(aero_element))
                            writer.Write("    -->\n");
                    }
                }
                
                if (coeffs.Any())
                    writer.Write("    <function name=\"aero/coefficients/{0}\">\n", coeffs.First().Substring(0,2));
                //writer.Write("    <description>{0}</description>\n", aero.GetDescription(aerodat_item));
                writer.Write("    <sum>\n");
                foreach (var coeff in coeffs)
                {
                    writer.Write("          <property>aero/coefficients/{0}</property>\n", coeff);
                }
                writer.Write("       </sum>\n");
                writer.Write("    </function>\n");

            }
            foreach (var aero_axis in aero.GetElements().GroupBy(xx => GetAxis(xx)))
            {
                writer.Write("  <axis name=\"{0}\">\n", aero_axis.Key);
                var coeff = GetVar(aero_axis.First()).Substring(0, 2);
                writer.Write("    <function name=\"aero/force/{0}\">\n", aero_axis.Key);
                writer.Write("    <description>{0} {1}</description>\n", aero_axis.Key, Char.IsLower(coeff.Substring(1,1).First()) ? "Force" : "Moment");
                writer.Write("    <product>\n");
                output_denormalisation(writer, coeff, aero_axis.Key);
                writer.Write("       </product>\n");
                writer.Write("    </function>\n");

                writer.Write("  </axis>\n");
            }
            writer.Write("</pre>\n");
        }

        private void OutputExtraIndependentVariables(HtmlTextWriter writer, string aero_element)
        {
            foreach (var xx in aero.GetExtraIndependantVariables(aero_element))
            {
                var name = xx;
                if (!name.Contains("/"))
                    name = "fcs/" + name;

                writer.Write("          <property>{0}</property>\n", name);
            }
        }

        private static void output_denormalisation(HtmlTextWriter writer, string aerodat_item, string aero_axis_key)
        {
            writer.Write("          <property>aero/qbar-psf</property>\n");
            writer.Write("          <property>metrics/Sw-sqft</property>\n");
            if (aero_axis_key == "ROLL" || aero_axis_key == "YAW")
            {
                writer.Write("          <property>metrics/bw-ft</property>\n");
            }
            else if (aero_axis_key == "PITCH")
            {
                writer.Write("          <property>metrics/cbarw-ft</property>\n");
            }
            writer.Write("          <property>aero/coefficients/{0}</property>\n", aerodat_item);
        }

        private string GetVar(string xx)
        {
            if (xx.Contains(":"))
            {
                var v = xx.Split(':');
                if (v.Length > 1)
                    return v[1];
            }
            return xx;
        }

        private string GetAxis(string xx)
        {
            if (xx.Contains(":"))
            {
                var v = xx.Split(':');
                return v[0];
            }
            return xx;
        }
    }
}