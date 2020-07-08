using leave_manager.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_manager.Contracts
{
    public interface ILeaveAllocationRepository : IRepositoryBase<LeaveAllocation>
    {
        Task<bool> CheckAllocation(int leaveTypeId, string employeeId);
        Task<ICollection<LeaveAllocation>> GetAllocationsByEmployee(string id);
        Task<LeaveAllocation> GetAllocationsByEmployeeAndType(string id, int leaveTypeId);
    }
}
