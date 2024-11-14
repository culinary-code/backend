﻿using System;
using DOM.Accounts;

namespace DAL.Accounts;

public interface IAccountRepository
{
    Account ReadAccount(Guid id);
    void UpdateAccount(Account account);
    void CreateAccount(Account account);
}