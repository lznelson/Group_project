using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Model;
using Common;
using System.ServiceModel;
using EmailService.Business.Components.Interfaces;


namespace EmailService.Services
{
    public class SubscriberService : ISubscriberService
    {
        [OperationBehavior(TransactionScopeRequired = true)]
        public void PublishToSubscriber(Message pMessage)
        {
            EmailMessage mEmailMessage = pMessage as EmailMessage;
            EmailProvider.SendEmail(
        MessageTypeConverter.Instance.Convert<
        global::Common.Model.EmailMessage,
        global::EmailService.Business.Entities.EmailMessage>(mEmailMessage)

    );
        }
        public IEmailProvider EmailProvider
        {
            get
            {
                return ServiceFactory.GetService<IEmailProvider>();
            }
        }


    }
}
