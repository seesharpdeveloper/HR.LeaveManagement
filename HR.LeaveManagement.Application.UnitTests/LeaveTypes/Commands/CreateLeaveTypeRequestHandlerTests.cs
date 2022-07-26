﻿using AutoMapper;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.DTOs.LeaveType;
using HR.LeaveManagement.Application.Features.LeaveTypes.Handlers.Commands;
using HR.LeaveManagement.Application.Features.LeaveTypes.Requests.Commands;
using HR.LeaveManagement.Application.Profiles;
using HR.LeaveManagement.Application.Responses;
using HR.LeaveManagement.Application.UnitTests.Mocks;
using Moq;
using Shouldly;

namespace HR.LeaveManagement.Application.UnitTests.LeaveTypes.Commands;

public class CreateLeaveTypeRequestHandlerTests
{
    private readonly IMapper _mapper;
    private readonly Mock<IUnitOfWork> _mockRepo;
    private readonly CreateLeaveTypeDto _leaveTypeDto;
    private readonly CreateLeaveTypeCommandHandler _handler;

    public CreateLeaveTypeRequestHandlerTests()
    {
        _mockRepo = MockUnitOfWork.GetUnitOfWork();

        var mapperConfig = new MapperConfiguration(c =>
        {
            c.AddProfile<MappingProfile>();
        });

        _mapper = mapperConfig.CreateMapper();
        _handler = new CreateLeaveTypeCommandHandler(_mockRepo.Object, _mapper);
        _leaveTypeDto = new CreateLeaveTypeDto()
        {
            Name = "Test DTO",
            DefaultDays = 17
        };
    }

    [Fact]
    public async Task Valid_LeaveType_Added()
    {
        var result = await _handler.Handle
            (
            new CreateLeaveTypeCommand() { LeaveTypeDto = _leaveTypeDto },
            CancellationToken.None
            );

        var leaveTypes = await _mockRepo.Object.LeaveTypeRepository.GetAll();

        result.ShouldBeOfType<BaseCommandResponse>();

        leaveTypes.Count.ShouldBe(4);
    }

    [Fact]
    public async Task InValid_LeaveType_Added()
    {
        _leaveTypeDto.DefaultDays = -17;

        var result = await _handler.Handle(new CreateLeaveTypeCommand() { LeaveTypeDto = _leaveTypeDto }, CancellationToken.None);

        var leaveTypes = await _mockRepo.Object.LeaveTypeRepository.GetAll();

        leaveTypes.Count.ShouldBe(3);

        result.ShouldBeOfType<BaseCommandResponse>();
    }
}
