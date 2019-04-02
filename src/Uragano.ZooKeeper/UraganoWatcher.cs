using System.Threading.Tasks;
using org.apache.zookeeper;

namespace Uragano.ZooKeeper
{
    delegate void ZooKeeperHandler(string path, Watcher.Event.KeeperState keeperState, Watcher.Event.EventType eventType);
    internal class UraganoWatcher : Watcher
    {
        public event ZooKeeperHandler OnChange;

        public override async Task process(WatchedEvent @event)
        {
            var state = @event.getState();
            var path = @event.getPath();
            var type = @event.get_Type();
            OnChange?.Invoke(path, state, type);
            await Task.CompletedTask;
        }
    }
}
