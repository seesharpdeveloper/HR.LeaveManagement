﻿using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Features.LeaveAllocations.Requests.Commands;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Domain;
using MediatR;

namespace HR.LeaveManagement.Application.Features.LeaveAllocations.Handlers.Commands
{
    public class DeleteLeaveAllocationCommandHandler : IRequestHandler<DeleteLeaveAllocationCommand>
    {
        public DeleteLeaveAllocationCommandHandler(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        public IUnitOfWork UnitOfWork { get; }

        public async Task<Unit> Handle(DeleteLeaveAllocationCommand request, CancellationToken cancellationToken)
        {
            var leaveAllocation = await UnitOfWork.LeaveAllocationRepository.Get(request.Id);

            if (leaveAllocation == null)
                throw new NotFoundException(nameof(LeaveAllocation), request.Id);

            await UnitOfWork.LeaveAllocationRepository.Delete(leaveAllocation);
            await UnitOfWork.Save();

            return Unit.Value;
        }
    }
}
