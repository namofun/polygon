﻿using MediatR;

namespace Polygon.Judgement
{
    public class NextJudgingRequest : IRequest<NextJudging?>
    {
        public string HostName { get; }

        public NextJudgingRequest(string hostname)
        {
            HostName = hostname;
        }
    }
}
