using leave_manager.Contracts;
using leave_manager.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace leave_manager.Repository
{
    public class LeaveRequestRepository : ILeaveRequestRepository
    {
        private readonly ApplicationDbContext _db;

        public LeaveRequestRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<bool> Create(LeaveRequest entity)
        {
            await _db.LeaveRequests.AddAsync(entity);
            return await Save();
        }

        public async Task<bool> Delete(LeaveRequest entity)
        {
            _db.LeaveRequests.Remove(entity);
            return await Save();
        }

        public async Task<ICollection<LeaveRequest>> FindAll()
        {
            var LeaveRequests = await _db.LeaveRequests.Include(q => q.RequestingEmployee).Include(q => q.ApprovedBy).Include(q => q.LeaveType).ToListAsync();
            return LeaveRequests;
        }

        public async Task<LeaveRequest> FindById(int id)
        {
            return await _db.LeaveRequests.Include(q => q.RequestingEmployee).Include(q => q.ApprovedBy).Include(q => q.LeaveType).FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<ICollection<LeaveRequest>> GetRequestsByEmployee(string id)
        {
            var requests = await FindAll();
            return requests.Where(q => q.RequestingEmployeeId == id).ToList();
        }

        public async Task<bool> isExists(int id)
        {
            var exists = await _db.LeaveRequests.AnyAsync(q => q.Id == id);
            return exists;
        }

        public async Task<bool> Save()
        {
            var changes = await _db.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> Update(LeaveRequest entity)
        {
            _db.LeaveRequests.Update(entity);
            return await Save();
        }
    }
}
