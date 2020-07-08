using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using leave_manager.Contracts;
using leave_manager.Data;
using leave_manager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace leave_manager.Controllers
{
    [Authorize]
    public class LeaveRequestController : Controller
    {
        private readonly ILeaveRequestRepository _requestRepo;
        private readonly ILeaveTypeRepository _typeRepo;
        private readonly ILeaveAllocationRepository _allocationRepo;
        private readonly IMapper _mapper;
        private readonly UserManager<Employee> _userManager;

        public LeaveRequestController(ILeaveRequestRepository requestRepo, ILeaveTypeRepository typeRepo, ILeaveAllocationRepository allocationRepo, IMapper mapper, UserManager<Employee> userManager)
        {
            _requestRepo = requestRepo;
            _typeRepo = typeRepo;
            _allocationRepo = allocationRepo;
            _mapper = mapper;
            _userManager = userManager;
        }

        // GET: LeaveRequestController
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> Index()
        {
            var requests = _mapper.Map<List<LeaveRequestViewModel>>(await _requestRepo.FindAll());
            var model = new AdminLeaveRequestViewModel
            {
                TotalRequests = requests.Count,
                ApprovedRequests = requests.Count(q => q.Approved == true),
                PendingRequests = requests.Count(q => q.Approved == null),
                RejecetdRequests = requests.Count(q => q.Approved == false),
                LeaveRequests = requests
            };
            return View(model);
        }

        // GET: LeaveRequestController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var request = await _requestRepo.FindById(id);
            var model = _mapper.Map<LeaveRequestViewModel>(request);
            return View(model);
        }

        public async Task<ActionResult> ApproveRequest(int id)
        {
            try
            {
                var request = await _requestRepo.FindById(id);
                var user = await _userManager.GetUserAsync(User);
                var allocation = await _allocationRepo.GetAllocationsByEmployeeAndType(request.RequestingEmployeeId, request.LeaveTypeId);
                request.Approved = true;
                request.ApprovedById = user.Id;
                //request.ApprovedBy.FirstName = user.FirstName;
                //request.ApprovedBy.LastName = user.LastName;
                request.DateActioned = DateTime.Now;
                int daysRequested = (int)(request.EndDate - request.StartDate).TotalDays;
                allocation.NumberOfDays -= daysRequested;

                await _requestRepo.Update(request);
                await _allocationRepo.Update(allocation);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }

        }

        public async Task<ActionResult> RejectRequest(int id)
        {
            try
            {
                var request = await _requestRepo.FindById(id);
                var user = await _userManager.GetUserAsync(User);
                request.Approved = false;
                request.ApprovedById = user.Id;
                //request.ApprovedBy.FirstName = user.FirstName;
                //request.ApprovedBy.LastName = user.LastName;
                request.DateActioned = DateTime.Now;

                await _requestRepo.Update(request);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: LeaveRequestController/Create
        public async Task<ActionResult> Create()
        {
            var leaveTypes = await _typeRepo.FindAll();
            var leaveTypeItems = leaveTypes.Select(q => new SelectListItem { Text = q.Name, Value = q.Id.ToString() });
            var model = new CreateLeaveRequestViewModel
            {
                LeaveTypes = leaveTypeItems
            };
            return View(model);
        }

        // POST: LeaveRequestController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateLeaveRequestViewModel model)
        {
            try
            {
                var startDate = Convert.ToDateTime(model.StartDate);
                var endDate = Convert.ToDateTime(model.EndDate);
                var leaveTypes = await _typeRepo.FindAll();
                var leaveTypeItems = leaveTypes.Select(q => new SelectListItem { Text = q.Name, Value = q.Id.ToString() });
                model.LeaveTypes = leaveTypeItems;

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (DateTime.Compare(startDate, endDate) > 1)
                {
                    ModelState.AddModelError("", "The start date cannot be greater than the end date!");
                    return View(model);
                }

                var employee = await _userManager.GetUserAsync(User);
                var allocation = await _allocationRepo.GetAllocationsByEmployeeAndType(employee.Id, model.LeaveTypeId);
                int daysRequested = (int)(endDate - startDate).TotalDays;

                if (daysRequested > allocation.NumberOfDays)
                {
                    ModelState.AddModelError("", "You do not have sufficient leave time for this request!");
                    return View(model);
                }

                var leaveRequestModel = new LeaveRequestViewModel
                {
                    LeaveTypeId = model.LeaveTypeId,
                    RequestingEmployeeId = employee.Id,
                    StartDate = startDate,
                    EndDate = endDate,
                    Approved = null,
                    DateRequested = DateTime.Now,
                    DateActioned = DateTime.Now,
                    RequestComments = model.RequestComments
                };

                var leaveRequest = _mapper.Map<LeaveRequest>(leaveRequestModel);
                var isSuccess = await _requestRepo.Create(leaveRequest);

                if (!isSuccess)
                {
                    ModelState.AddModelError("", "Unable to submit record! Please try another request.");
                    return View(model);
                }

                return RedirectToAction("MyLeave");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Something went wrong! Please try another request.");
                return View(model);
            }
        }

        public async Task<ActionResult> MyLeave()
        {
            var employee = await _userManager.GetUserAsync(User);
            var employeeId = employee.Id;
            var employeeAllocations = await _allocationRepo.GetAllocationsByEmployee(employeeId);
            var employeeRequests = await _requestRepo.GetRequestsByEmployee(employeeId);
            var employeeAllocationsModel = _mapper.Map<List<LeaveAllocationViewModel>>(employeeAllocations);
            var employeeRequestsModel = _mapper.Map<List<LeaveRequestViewModel>>(employeeRequests);

            var model = new EmployeeLeaveRequestViewModel
            {
                LeaveAllocations = employeeAllocationsModel,
                LeaveRequests = employeeRequestsModel
            };

            return View(model);
        }

        public async Task<ActionResult> CancelRequest(int id)
        {
            var request = await _requestRepo.FindById(id);
            request.Cancelled = true;
            await _requestRepo.Update(request);
            return RedirectToAction("MyLeave");
        }

        // GET: LeaveRequestController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: LeaveRequestController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LeaveRequestController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
