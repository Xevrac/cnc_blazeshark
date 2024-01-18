﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazeLibWV
{
    public static class BlazePrettyPrinter
    {
        public static string PrintPacket(Blaze.Packet p)
        {
            if (p == null)
            {
                // Handle where packet null
                return "Packet is null";
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[{0}][{1}:{2}] {3}", Blaze.PacketToDescriber(p), p.Component.ToString("X4"), p.Command.ToString("X4"), p.QType == 0 ? "from client" : "from server");
            sb.AppendLine();
            sb.AppendLine("{");
            List<Blaze.Tdf> content = Blaze.ReadPacketContent(p);
            foreach (Blaze.Tdf tdf in content)
                sb.Append(PrintTdf(tdf, 1));
            sb.AppendLine("}");
            return sb.ToString();
        }

        public static string PrintTdf(Blaze.Tdf tdf, int tabs)
        {
            if (tdf == null)
            {
                return "ERR : Tdf returned null";
            }

            StringBuilder sb = new StringBuilder();
            string tab = "";
            for (int i = 0; i < tabs; i++)
                tab += "\t";
            sb.Append(tab + "[" + tdf.Label + "]:" + tdf.GetTypeDesc() + " = ");
            long n;
            string s;
            Blaze.DoubleVal dv;
            List<Blaze.Tdf> tdfl;
            switch (tdf.Type)
            {
                case 0:
                    Blaze.TdfInteger tdfInteger = tdf as Blaze.TdfInteger;
                    if (tdfInteger != null)
                    {
                        n = tdfInteger.Value;
                        sb.AppendLine("0x" + n.ToString("X") + "(" + n + ")");
                    }
                    break;

                case 1:
                    Blaze.TdfString tdfString = tdf as Blaze.TdfString;
                    if (tdfString != null)
                    {
                        s = tdfString.Value;
                        sb.AppendLine("\"" + s + "\"");
                    }
                    break;

                case 3:
                    Blaze.TdfStruct tdfStruct = tdf as Blaze.TdfStruct;
                    if (tdfStruct != null)
                    {
                        tdfl = tdfStruct.Values;
                        sb.AppendLine();
                        if (tdfStruct.startswith2)
                            sb.AppendLine(tab + "{(*starts with 0x02)");
                        else
                            sb.AppendLine(tab + "{");
                        foreach (Blaze.Tdf tdf2 in tdfl)
                            sb.Append(PrintTdf(tdf2, tabs + 1));
                        sb.AppendLine(tab + "}");
                    }
                    break;

                case 4:
                    Blaze.TdfList tdfList = tdf as Blaze.TdfList;
                    if (tdfList != null)
                    {
                        sb.AppendLine();
                        sb.AppendLine(tab + "{");
                        sb.Append(PrintTdfList(tdfList, tabs + 1));
                        sb.AppendLine(tab + "}");
                    }
                    break;

                case 5:
                    Blaze.TdfDoubleList tdfDoubleList = tdf as Blaze.TdfDoubleList;
                    if (tdfDoubleList != null)
                    {
                        sb.AppendLine();
                        sb.AppendLine(tab + "{");
                        sb.Append(PrintTdfDoubleList(tdfDoubleList, tabs + 1));
                        sb.AppendLine(tab + "}");
                    }
                    break;

                case 6:
                    Blaze.TdfUnion tdfUnion = tdf as Blaze.TdfUnion;
                    if (tdfUnion != null && tdfUnion.Type != 0x7f)
                    {
                        sb.AppendLine();
                        sb.AppendLine(tab + "{");
                        sb.Append(PrintTdf(tdfUnion.UnionContent, tabs + 1));
                        sb.AppendLine(tab + "}");
                    }
                    break;

                case 8:
                    Blaze.TdfDoubleVal tdfDoubleVal = tdf as Blaze.TdfDoubleVal;
                    if (tdfDoubleVal != null)
                    {
                        dv = tdfDoubleVal.Value;
                        sb.AppendLine("{0x" + dv.v1.ToString("X") + "(" + dv.v1 + "),0x" + dv.v2.ToString("X") + "(" + dv.v2 + ")}");
                    }
                    break;

                default:
                    sb.AppendLine();
                    break;
            }
            return sb.ToString();
        }

        public static string PrintTdfList(Blaze.TdfList list, int tabs)
        {
            StringBuilder sb = new StringBuilder();
            string tab = "";
            for (int i = 0; i < tabs; i++)
                tab += "\t";
            List<long> li;
            List<string> ls;
            List<Blaze.TdfStruct> lt;
            int count = 0;
            switch (list.SubType)
            {
                case 0:
                    li = ((List<long>)list.List);
                    foreach (long l in li)
                        sb.AppendLine(tab + "0x" + l.ToString("X") + "(" + l + "),");
                    break;
                case 1:
                    ls = ((List<string>)list.List);
                    foreach (string s in ls)
                        sb.AppendLine(tab + "\"" + s + "\",");
                    break;
                case 3:
                    lt = ((List<Blaze.TdfStruct>)list.List);
                    foreach (Blaze.TdfStruct tdf in lt)
                    {
                        tdf.Type = 3;
                        tdf.Label = (count++).ToString();
                        sb.Append(PrintTdf(tdf, tabs));
                    }
                    break;
            }
            return sb.ToString();
        }

        public static string PrintTdfDoubleList(Blaze.TdfDoubleList list, int tabs)
        {
            StringBuilder sb = new StringBuilder();
            string tab = "";
            for (int i = 0; i < tabs; i++)
                tab += "\t";
            long l;
            string s;
            Blaze.TdfStruct t;
            for (int i = 0; i < list.Count; i++)
            {
                switch (list.SubType1)
                {
                    case 0:
                        l = ((List<long>)list.List1)[i];
                        sb.Append(tab + "0x" + l.ToString("X") + "(" + l + ") = ");
                        break;
                    case 1:
                        s = ((List<string>)list.List1)[i];
                        sb.Append(tab + "\"" + s + "\" = ");
                        break;
                    default:
                        sb.Append(tab + " = ");
                        break;
                }
                switch (list.SubType2)
                {
                    case 0:
                        l = ((List<long>)list.List2)[i];
                        sb.AppendLine("0x" + l.ToString("X") + "(" + l + "),");
                        break;
                    case 1:
                        s = ((List<string>)list.List2)[i];
                        sb.AppendLine("\"" + s + "\",");
                        break;
                    case 3:
                        t = ((List<Blaze.TdfStruct>)list.List2)[i];
                        sb.AppendLine(" [" + i + "]:TdfStruct{");
                        foreach (Blaze.Tdf tdf in t.Values)
                            sb.Append(PrintTdf(tdf, tabs + 1));
                        sb.AppendLine(tab + "},");
                        break;
                    default:
                        sb.AppendLine(",");
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
