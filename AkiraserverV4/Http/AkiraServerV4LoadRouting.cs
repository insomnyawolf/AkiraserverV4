﻿using AkiraserverV4.Http.BaseContext.Requests;
using AkiraserverV4.Http.Exceptions;
using AkiraserverV4.Http.Helper;
using AkiraserverV4.Http.Model;
using Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using static AkiraserverV4.Http.BaseContext.Ctx;

namespace AkiraserverV4.Http
{
    public partial class AkiraServerV4
    {
        private Endpoint[] Endpoints;

        public void LoadRouting(Assembly assembly)
        {
            if (assembly is null)
            {
                return;
            }

            List<Endpoint> endpoints = new List<Endpoint>();

            Type[] classes = assembly.GetTypes();
            for (int classIndex = 0; classIndex < classes.Length; classIndex++)
            {
                Type currentClass = classes[classIndex];
                ControllerAttribute controllerAttribute = currentClass.GetCustomAttribute<ControllerAttribute>();
                if (controllerAttribute != null)
                {
                    MethodInfo[] methods = currentClass.GetMethods();
                    for (int methodIndex = 0; methodIndex < methods.Length; methodIndex++)
                    {
                        MethodInfo currentMethod = methods[methodIndex];

                        IEnumerable<Attribute> Attributes = currentMethod.GetCustomAttributes();

                        foreach (Attribute currentAttribute in Attributes)
                        {
                            if (currentAttribute is BaseEndpointAttribute endpointAttribute)
                            {
                                string controllerPath = controllerAttribute.Path.Replace("[controller]", currentClass.Name.RemoveAtEnd("Context"));
                                string methodPath = endpointAttribute.Path.Replace("[method]", currentMethod.Name);
                                string path = controllerPath + methodPath;
                                endpoints.Add(new Endpoint()
                                {
                                    ClassExecuted = currentClass,
                                    MethodExecuted = currentMethod.CreateReflectedDelegate(),
                                    Method = endpointAttribute.Method,
                                    Path = path,
                                    Priority = CalculatePriority(path),
                                    ReturnIsGenericType = currentMethod.ReturnType.IsGenericType
                                });
                            }
                        }
                    }
                }
            }

            static int CalculatePriority(string path)
            {
                return path.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;
            }

            // Ordena los endpoints de mas especificos a menos especificos
            endpoints.Sort((x, y) =>
            {
                if (x.Priority > y.Priority)
                {
                    return -1; //normally greater than = 1
                }

                if (x.Priority < y.Priority)
                {
                    return 1; // normally smaller than = -1
                }
                return 0; // equal
            });

            ValidateRouting(ref endpoints);

            Endpoints = endpoints.ToArray();

            Logger.LogInformation(LogRoutingInfo());
        }

        private class EndpointCount
        {
            public string Path { get; set; }
            public HttpMethod Method { get; set; }
            public int Count { get; set; }
        }

        private void ValidateRouting(ref List<Endpoint> endpoints)
        {
            List<EndpointCount> duplicatedCheck = new List<EndpointCount>();

            for (int index = 0; index < endpoints.Count; index++)
            {
                Endpoint endpoint = endpoints[index];

                bool exists = false;

                for (int duplicatedCheckIndex = 0; duplicatedCheckIndex < duplicatedCheck.Count; duplicatedCheckIndex++)
                {
                    if (endpoint.Path.Equals(duplicatedCheck[duplicatedCheckIndex].Path, StringComparison.InvariantCultureIgnoreCase)
                        && endpoint.Method == duplicatedCheck[duplicatedCheckIndex].Method)
                    {
                        exists = true;
                        duplicatedCheck[duplicatedCheckIndex].Count++;
                    }
                }

                if (!exists)
                {
                    duplicatedCheck.Add(new EndpointCount()
                    {
                        Path = endpoint.Path,
                        Method = endpoint.Method
                    });
                }
            }

            StringBuilder error = new StringBuilder();
            for (int i = 0; i < duplicatedCheck.Count; i++)
            {
                EndpointCount item = duplicatedCheck[i];
                if (item.Count > 0)
                {
                    // $"* Route: '{item.Method} => {item.Path} ' appears '{item.Count + 1}' times .\n"
                    error.Append("* Route: '").Append(item.Method.Padding(7, position: PaddingPosition.Left)).Append(" => ").Append(item.Path).Append(" ' appears '").Append(item.Count + 1).Append("' times .\n");
                }
            }

            string errorString = error.ToString();
            if (!string.IsNullOrEmpty(errorString))
            {
                throw new RoutingException(errorString);
            }
        }

        private string LogRoutingInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Loaded The following Endpoints:\n");
            foreach (Endpoint endpoint in Endpoints)
            {
                sb.Append("\t\t* Route: '").Append(endpoint.Method.Padding(7, position: PaddingPosition.Left)).Append(" => ").Append(endpoint.Path).Append("'.\n");
            }

            return sb.ToString();
        }
    }
}