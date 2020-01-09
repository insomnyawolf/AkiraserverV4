using AkiraserverV4.Http.Exceptions;
using AkiraserverV4.Http.Extensions;
using AkiraserverV4.Http.Model;
using AkiraserverV4.Http.SerializeHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AkiraserverV4.Http.BaseContex.Requests;
using AkiraserverV4.Http.BaseContex.Responses;
using AkiraserverV4.Http.BaseContex;
using static AkiraserverV4.Http.BaseContex.BaseContext;

namespace AkiraserverV4.Http
{
    public partial class Listener
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

                        if (currentMethod.GetCustomAttribute<NotFoundHandlerAttribute>() != null)
                        {
                            if (NotFound != null)
                            {
                                throw new MultipleMatchException(nameof(NotFoundHandlerAttribute));
                            }
                            NotFound = new ExecutedCommand() { MethodExecuted = currentMethod, ClassExecuted = currentClass };
                        }
                        else if (currentMethod.GetCustomAttribute<InternalServerErrorHandlerAttribute>() != null)
                        {
                            if (InternalServerError != null)
                            {
                                throw new MultipleMatchException(nameof(InternalServerErrorHandlerAttribute));
                            }
                            InternalServerError = new ExecutedCommand() { MethodExecuted = currentMethod, ClassExecuted = currentClass };
                        }
                        else if (currentMethod.GetCustomAttribute<BaseEndpointAttribute>() is BaseEndpointAttribute endpointAttribute)
                        {
                            string controllerPath = controllerAttribute.Path.Replace("[controller]", currentClass.Name);
                            string methodPath = endpointAttribute.Path.Replace("[method]", currentMethod.Name);
                            string path = controllerPath + methodPath;
                            endpoints.Add(new Endpoint()
                            {
                                ClassExecuted = currentClass,
                                MethodExecuted = currentMethod,
                                Method = endpointAttribute.Method,
                                Path = path,
                                Priority = CalculatePriority(path)
                            });
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
                    // $"* Route: '{item.Method} => {item.Path} ' appears '{item.Count + 1}' times ."
                    error.Append("* Route: '").Append(item.Method).Append(" => ").Append(item.Path).Append(" ' appears '").Append(item.Count + 1).Append("' times .");
                }
            }

            string errorString = error.ToString();

            if (!string.IsNullOrEmpty(errorString))
            {
                throw new RoutingException(errorString);
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("Loaded The following Endpoints:\n");
            foreach (Endpoint endpoint in endpoints)
            {
                sb.Append("* Route: '").Append(endpoint.Method).Append(" => ").Append(endpoint.Path).Append("'.\n");
            }
            Console.WriteLine(sb.ToString());
        }
    }
}