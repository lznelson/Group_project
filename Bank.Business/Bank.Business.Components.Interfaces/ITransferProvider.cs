using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Model;

namespace Bank.Business.Components.Interfaces
{
    public interface ITransferProvider
    {
        void Transfer(Message pMessage);
    }
}
