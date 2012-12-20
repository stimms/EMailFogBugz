using System.Net.Mail;

namespace EMailFogBugz
{
    /// <summary>
    /// An interface used to expose SmtpClient methods.
    /// </summary>
    public interface ISmtpClient
    {
        #region Public Methods
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="message">The message describing the email to send.</param>
        /// <param name="userToken">An optional user token.</param>
        void SendAsync(MailMessage message, object userToken = null);
        #endregion
    }
}