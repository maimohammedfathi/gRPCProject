using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using ToDoGrpc.Data;
using ToDoGrpc.Models;
using ToDoGrpc.Protos;

namespace ToDoGrpc.Services
{
    public class ToDoService : ToDoIt.ToDoItBase
    {
        private readonly AppDbContext _dbcontext;

        public ToDoService(AppDbContext dbcontext)
        {
            _dbcontext=dbcontext;
        }
        public override async Task<CreateToDoResponse> CreateToDo(CreateToDoRequest request, ServerCallContext context)
        {
            if (request.Title == string.Empty || request.Description == string.Empty)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));
            var toDoItem = new ToDoItem
            {
                Title = request.Title,
                Description = request.Description,
            };
            await _dbcontext.AddAsync(toDoItem);
            await _dbcontext.SaveChangesAsync();

            return await Task.FromResult(new CreateToDoResponse
            {
                Id = toDoItem.Id
            });
        }
         
        public override async Task<ReadToDoResponse> ReadToDo(ReadToDoRequest request,ServerCallContext context)
        {
            if (request.Id <= 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Resource index must be greater than zero"));

            var toDoItem = await _dbcontext.toDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);
            if(toDoItem != null)
            {
                return await Task.FromResult(new ReadToDoResponse
                {
                    Id = toDoItem.Id,
                    Title = toDoItem.Title,
                    Description = toDoItem.Description,
                    ToDoStatus = toDoItem.ToDoStatus
                });
            }
            throw new RpcException(new Status(StatusCode.NotFound, $"Not Task with Id {request.Id}"));
        }

        public override async Task<GetAllResponse> ListToDo(GetAllRequest request, ServerCallContext context)
        {
            var response = new GetAllResponse();
            var toDoItems = await _dbcontext.toDoItems.ToListAsync();

            foreach (var toDoItem in toDoItems)
            {
                response.ToDo.Add(new ReadToDoResponse
                {
                    Id = toDoItem.Id,
                    Title = toDoItem.Title,
                    Description = toDoItem.Description,
                    ToDoStatus = toDoItem.ToDoStatus
                });
            }
            return await Task.FromResult(response);
        }

        public override async Task<UpdateToDoResponse> UpdateToDo(UpdateToDoRequest request, ServerCallContext context)
        {
            if(request.Id <= 0 || request.Title == string.Empty || request.Description == string.Empty)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "You must supply a valid object"));

            var toDoItem = await _dbcontext.toDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

            if (toDoItem == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"No task with Id {request.Id}"));

            toDoItem.Title = request.Title;
            toDoItem.Description = request.Description;
            toDoItem.ToDoStatus = request.ToDoStatus;

            await _dbcontext.SaveChangesAsync();

            return await Task.FromResult(new UpdateToDoResponse
            { 
                Id = toDoItem.Id
            
            });
        }
        public override async Task<DeleteToDoResponse> DeleteToDo(DeleteToDoRequest request, ServerCallContext context)
        {
            if (request.Id <= 0)
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Resource index must be greater than zero"));

            var toDoItem = await _dbcontext.toDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);

            if (toDoItem == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"No Task With Id {request.Id}"));

            _dbcontext.Remove(toDoItem);
            _dbcontext.SaveChangesAsync();

            return await Task.FromResult(new DeleteToDoResponse
            { Id = request.Id });
        }
    }
}
