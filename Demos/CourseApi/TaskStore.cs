namespace CourseApi;

// Temporary classroom storage. Data resets when the application restarts.
// The lock protects this shared store if requests arrive together.
public class TaskStore
{
    private readonly object gate = new();
    private readonly List<TaskItem> tasks = new();
    private int nextId = 1;

    private static TaskItem Copy(TaskItem task)
    {
        return new TaskItem { Id = task.Id, Title = task.Title,
            IsCompleted = task.IsCompleted };
    }

    public TaskItem[] All(bool? completed)
    {
        lock (gate)
        {
            return tasks.Where(task => completed is null ||
                task.IsCompleted == completed).Select(Copy).ToArray();
        }
    }

    public TaskItem? Find(int id)
    {
        lock (gate)
        {
            var task = tasks.Find(task => task.Id == id);
            return task is null ? null : Copy(task);
        }
    }

    public TaskItem Add(TaskWriteRequest request)
    {
        lock (gate)
        {
            var task = new TaskItem { Id = nextId++, Title = request.Title.Trim(),
                IsCompleted = request.IsCompleted };
            tasks.Add(task);
            return Copy(task);
        }
    }

    public bool Update(int id, TaskWriteRequest request)
    {
        lock (gate)
        {
            var task = tasks.Find(task => task.Id == id);
            if (task is null) return false;
            task.Title = request.Title.Trim();
            task.IsCompleted = request.IsCompleted;
            return true;
        }
    }

    public bool Remove(int id)
    {
        lock (gate)
        {
            return tasks.RemoveAll(task => task.Id == id) > 0;
        }
    }
}
