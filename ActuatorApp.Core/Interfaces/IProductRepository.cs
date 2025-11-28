using System.Collections.Generic;
using System.Threading.Tasks;
using ActuatorApp.Core.Entities;

namespace ActuatorApp.Core.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Tcsync>> GetTcsyncsAsync();
        Task<IEnumerable<Touch>> GetTouchesAsync();
        Task<IEnumerable<TCS>> GetTCSsAsync();
        Task<IEnumerable<Controlbox>> GetControlBoxesAsync();
        Task<IEnumerable<Control>> GetControlsAsync();
        Task<IEnumerable<Actuator>> GetActuatorsAsync();
        Task<IEnumerable<Columns>> GetColumnsAsync();
        Task<IEnumerable<Actlevel>> GetActlevelsAsync();
    }
}
