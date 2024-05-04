﻿namespace Blog.Interfaces
{
    public interface IEmailRepository
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
