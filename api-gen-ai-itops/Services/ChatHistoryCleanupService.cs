namespace api_gen_ai_itops.Services
{
    public class ChatHistoryCleanupService : BackgroundService
    {
        private readonly IChatHistoryManager _chatHistoryManager;
        private readonly TimeSpan _interval = TimeSpan.FromHours(1); // Adjust as needed

        public ChatHistoryCleanupService(IChatHistoryManager chatHistoryManager)
        {
            _chatHistoryManager = chatHistoryManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _chatHistoryManager.CleanupOldHistories();
                await Task.Delay(_interval, stoppingToken);
            }
        }
    }
}