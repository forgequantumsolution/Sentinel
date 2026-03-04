using Analytics_BE.Core.Enums;

namespace Analytics_BE.Core.Interfaces
{
    public interface IRequestFlow
    {
        public RequestStatus Status { get; set; }
    }
}
