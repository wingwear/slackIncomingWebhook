using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using System.ComponentModel;
using System.Net;
using System.Collections.Specialized;

namespace SlackIncomingWebhook
{
    public class SlackIncomingWebhook : CodeActivity
    {
        [Category("Input")]
        [RequiredArgument]
        [Description("Enter the income webhook")]
        public InArgument<string> IncomingWebHook { get; set; }

        [Category("Input")]
        [RequiredArgument]
        [Description("Enter the message content")]
        public InArgument<string> Message { get; set; }

        [Category("Input")]
        [Description("Enter the channel name, overriden by person")]
        public InArgument<string> Channel { get; set; }

        [Category("Input")]
        [Description("Enter the person's name to send message to")]
        public InArgument<string> Person { get; set; }

        [Category("Input")]
        [Description("Enter the sender's name to display")]
        public InArgument<string> Sender { get; set; }

        [Category("Output")]
        [Description("Output of this post")]
        public OutArgument<string> Status { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            var incomingWebHook = IncomingWebHook.Get(context).Trim();
            var message = Message.Get(context);

            //Verify channel or person is provided
            if ((Channel.Get(context) == null) & (Person.Get(context) == null))
            {
                throw new System.ArgumentException("Please enter either channel or person");
            }

            //Prepare and post http request
            using (var client = new WebClient())
            {
                //Init the http request value
                var http_post_values = new NameValueCollection();
                http_post_values["payload"] = "{"; // {\"channel\": \"#" + channel + "\",  ,\"username\": \"" + sender + "\", \"text\": \"" + message + " \"}";

                //Start building the payload
                //Add channel or person
                if (Person.Get(context) == null)
                {
                    http_post_values["payload"] += "\"channel\": \"#" + Channel.Get(context).Trim() + "\"";
                }
                else
                {
                    http_post_values["payload"] += "\"channel\": \"@" + Person.Get(context).Trim() + "\"";
                }

                //Add sender if provided
                if (Sender.Get(context) != null)
                {
                    http_post_values["payload"] += ", \"username\": \"" + Sender.Get(context).Trim() + "\"";
                }
                
                //Add message
                http_post_values["payload"] += ", \"link_names\": \"1\", \"text\": \"" + message + "\"";

                //Wrapping up the payload
                http_post_values["payload"] += "}";

                //Posting http request
                var response = client.UploadValues(incomingWebHook, http_post_values);

                //Getting post response
                var responseString = Encoding.Default.GetString(response);
                Status.Set(context, responseString);
            }
        }
    }

}
