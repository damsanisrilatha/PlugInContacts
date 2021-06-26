using Microsoft.Xrm.Sdk;
using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlugInContacts
{
    public class TaskCreation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            // Then write the business logic
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)
            serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Obtain the Organization Service Reference

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                if (context.InputParameters.Contains("Target") &&
                       context.InputParameters["Target"] is Entity)
                {
                    // obtain the target entity from the input parameters
                    Entity entity = (Entity)context.InputParameters["Target"];

                    //Verify the target entity represents an account.
                    //If not, This Plug-in was not registered correctly.
                    if (entity.LogicalName != "account")
                        return;

                    EntityReference taskRegarding = null;
                    if (context.OutputParameters.Contains("id"))
                    {
                        Guid regardingobjectId = new Guid(context.OutputParameters["id"].ToString()); // GUID

                        string regardingobjectidType = "account";

                        taskRegarding = new EntityReference(regardingobjectidType, regardingobjectId);

                        Guid createRecordId = createEntityRecord("task", taskRegarding, service);
                    }
                }
            }

            catch (Exception ex)
            {

            }
        }
        public Guid createEntityRecord(string entityName, EntityReference regardingObject, IOrganizationService crmService)
        {

            //Create a task activity to follow up with the account customer in 7 days.
            Entity followup = new Entity(entityName); // Task
            followup["subject"] = "Send E-mail to the new customer";
            followup["description"] = "Follow Up with the customer. Check if there are any new issues that need  resolution.";
            followup["scheduledstart"] = DateTime.Now.AddDays(7);
            followup["scheduledend"] = DateTime.Now.AddDays(7);
            followup["category"] = regardingObject.LogicalName; // account
            followup["rgardingobjectid"] = regardingObject;    //  account Guid

            return crmService.Create(followup);
        }
    }
}

       
  