using MediatR;
using System.Collections.Generic;
using System.Net;

namespace Xylab.Polygon.Judgement.Requests
{
    public class JudgehostRegisterRequest : IRequest<List<UnfinishedJudging>>
    {
        public string HostName { get; }

        public string UserName { get; }

        public IPAddress Ip { get; }

        public JudgehostRegisterRequest(string hostname, string username, IPAddress ip)
        {
            HostName = hostname;
            UserName = username;
            Ip = ip;
        }
    }
}
