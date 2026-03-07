using Core.Enums;

namespace Core.Interfaces
{
    public interface IRequestFlow
    {
        public RequestStatus Status { get; set; }
    }
}
