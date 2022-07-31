using System.Threading.Tasks;
using MimeKit;

namespace Backend.Services;

public interface IMailService
{
    Task sendEmail(string toName, string toAddress, string subject, BodyBuilder body);
}