using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using Marten.Util;

namespace Marten.Events.Projections
{
    public class AggregatorApplyDefaultInterfaceMethods<T>:Aggregator<T> where T : class, new()
    {
        public AggregatorApplyDefaultInterfaceMethods()
        {
            var overrideMethodLookup = GetInterfaceDefaultMethods();
            Alias = typeof(T).Name.ToTableAlias();
            foreach (var method in overrideMethodLookup)
            {
                object step = null;
                var eventType = method.GetParameters().Single().ParameterType;
                if (eventType.Closes(typeof(Event<>)))
                {
                    eventType = eventType.GetGenericArguments().Single();
                    step = typeof(EventAggregationStep<,>)
                        .CloseAndBuildAs<object>(method, typeof(T), eventType);
                }
                else
                {
                    step = typeof(EventAggregationStep<,>)
                        .CloseAndBuildAs<object>(method, typeof(T), eventType);
                }

                Aggregations.Add(eventType, step);
            }
        }

        private static IEnumerable<MethodInfo> GetInterfaceDefaultMethods()
        {
            var applyMethods = new List<MethodInfo>();
            foreach (var i in typeof(T).GetInterfaces())
            {
                applyMethods.AddRange(
                    i.GetMethods().Where(x => x.Name == ApplyMethod && x.GetParameters().Length == 1)
                );
            }

            return applyMethods;
        }
    }
}
