﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Globalization;

namespace Lextm.SharpSnmpLib.Agent
{
    internal class Logger : IDisposable
    {
        private readonly StreamWriter _writer;

        public Logger()
        {
            _writer = new StreamWriter(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "snmp.log"), true);
            _writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "#Software: #SNMP Suite {0}", Assembly.GetEntryAssembly().GetName().Version));
            _writer.WriteLine("#Version: 1.0");
            _writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "#Date: {0}", DateTime.UtcNow));
            _writer.WriteLine("#Fields: date time s-ip cs-method cs-uri-stem s-port cs-username c-ip sc-status time-taken");
            _writer.AutoFlush = true;
        }

        public void Log(SnmpContext context)
        {
            TimeSpan timeTaken = DateTime.Now.Subtract(context.CreatedTime);
            _writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} {3} {4} {5} {6} {7} {8}",
                                            DateTime.UtcNow,
                                            "-",
                                            context.Request.Pdu.TypeCode,
                                            GetStem(context.Request.Pdu.Variables),
                                            context.Listener.Port,
                                            context.Request.Parameters.UserName,
                                            context.Sender.Address,
                                            (context.Response == null)
                                                ? "-"
                                                : context.Response.Pdu.ErrorStatus.ToErrorCode().ToString(),
                                            timeTaken));
        }

        private static string GetStem(IEnumerable<Variable> list)
        {
            StringBuilder result = new StringBuilder();
            foreach (Variable v in list)
            {
                result.AppendFormat("{0};", v.Id);
            }

            if (result.Length > 0)
            {
                result.Length--;
            }

            return result.ToString();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
