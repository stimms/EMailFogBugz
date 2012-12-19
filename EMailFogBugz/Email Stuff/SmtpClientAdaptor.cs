using System;
using System.Net.Mail;

namespace EMailFogBugz
{
    /// <summary>
    /// Adapts an SmtpClient to an ISmtpClient.
    /// </summary>
    public class SmtpClientAdapter : ISmtpClient
    {
        #region Data Members
        /// <summary>
        /// The underlying SmtpClient.
        /// </summary>
        private readonly SmtpClient _client;
        #endregion

        #region Constructors
        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="client">The underlying SmtpClient.</param>
        public SmtpClientAdapter(SmtpClient client = null)
        {
            if (client == null)
                client = new SmtpClient();

            _client = client;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="message">The message describing the email to send.</param>
        /// <param name="userToken">An optional user token.</param>
        public void SendAsync(MailMessage message, object userToken = null)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            _client.SendAsync(message, userToken);
        }
        #endregion
    }
}