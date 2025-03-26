using SEIDOR_SLayer;

namespace BlazorTemplateService.Handlers
{
    public class SLHandler
    {
        private SLConnection _connection;
        public SLHandler(SLConnection sLConnection) 
        {
            _connection = sLConnection;
        }
    }
}
