﻿using AutoMapper;
using HR.LeaveManagement.Application.DTOs.LeaveType.Validators;
using HR.LeaveManagement.Application.Features.LeaveAllocations.Requests.Commands;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Domain;
using MediatR;
using HR.LeaveManagement.Application.Responses;
using HR.LeaveManagement.Application.Contracts.Identity;

namespace HR.LeaveManagement.Application.Features.LeaveAllocations.Handlers.Commands
{
    public class CreateLeaveAllocationCommandHandler : IRequestHandler<CreateLeaveAllocationCommand, BaseCommandResponse>
    {
        public CreateLeaveAllocationCommandHandler(IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserService userService)
        {
            UnitOfWork = unitOfWork;
            Mapper = mapper;
            UserService = userService;
        }

        public IUserService UserService { get; }
        public IUnitOfWork UnitOfWork { get; }
        public IMapper Mapper { get; }

        public async Task<BaseCommandResponse> Handle(CreateLeaveAllocationCommand request, CancellationToken cancellationToken)
        {
            var response = new BaseCommandResponse();

            var validator = new CreateLeaveAllocationDtoValidator(UnitOfWork.LeaveTypeRepository);
            var validationResult = await validator.ValidateAsync(request.LeaveAllocationDto, cancellationToken);

            if (!validationResult.IsValid)
            {
                response.Success = false;
                response.Message = "Creation failed!";
                response.Errors = validationResult.Errors.Select(x => x.ErrorMessage).ToList();
                return response;
            }

            var leaveType = await UnitOfWork.LeaveTypeRepository.Get(request.LeaveAllocationDto.LeaveTypeId);
            var employees = await UserService.GetEmployees();
            var period = DateTime.Now.Year;
            var allocations = new List<LeaveAllocation>();

            foreach (var item in employees)
            {
                if (await UnitOfWork.LeaveAllocationRepository.AllocationExists(item.Id, leaveType.Id, period))
                    continue;
                allocations.Add(new LeaveAllocation
                {
                    LeaveTypeId = leaveType.Id,
                    EmployeeId = item.Id,
                    NumberOfDays = leaveType.DefaultDays,
                    Period = period
                });
            }

            if(allocations.Count > 0)
                await UnitOfWork.LeaveAllocationRepository.AddAllocations(allocations);

            await UnitOfWork.Save();

            //var leaveAllocation = Mapper.Map<LeaveAllocation>(request.LeaveAllocationDto);
            //leaveAllocation = await LeaveAllocationRepository.Add(leaveAllocation);

            response.Success = true;
            response.Message = "Creation Successful!";
            return response;
        }
    }
}
