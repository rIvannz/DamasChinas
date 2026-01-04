using DamasChinas_Server.Common;
using System;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace DamasChinas_Server.Logic
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DbGuardBehaviorAttribute : Attribute, IServiceBehavior
    {
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase,
            System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
            {
                foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                {
                    endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new DbGuardMessageInspector());
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }

        private sealed class DbGuardMessageInspector : IDispatchMessageInspector
        {
            public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
            {
                try
                {
                    DbGuard.EnsureDatabaseAvailable();
                    return null;
                }
                catch (RepositoryValidationException ex)
                {
                    DbOutageCoordinator.Trip(ex);
                    throw new FaultException<MessageCode>(ex.Code);
                }
                catch (SqlException ex)
                {
                    DbOutageCoordinator.Trip(ex);
                    throw new FaultException<MessageCode>(MessageCode.DatabaseUnavailable);
                }
                catch (EntityException ex)
                {
                    DbOutageCoordinator.Trip(ex);
                    throw new FaultException<MessageCode>(MessageCode.DatabaseUnavailable);
                }
                catch (TimeoutException ex)
                {
                    DbOutageCoordinator.Trip(ex);
                    throw new FaultException<MessageCode>(MessageCode.DatabaseUnavailable);
                }
                catch (Exception ex)
                {
                    DbOutageCoordinator.Trip(ex);
                    throw new FaultException<MessageCode>(MessageCode.DatabaseUnavailable);
                }
            }


            public void BeforeSendReply(ref Message reply, object correlationState)
            {
            }
        }
    }
}
