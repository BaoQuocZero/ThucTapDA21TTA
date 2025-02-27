﻿using demo_02.Models;
using Microsoft.EntityFrameworkCore;

public class EventService
{
    private readonly EventManagementContext _context;

    public EventService(EventManagementContext context)
    {
        _context = context;
    }

    // GetAll
    public async Task<List<Event>> GetAllEventsAsync()
    {
        return await _context.Events
            .Where(e => e.IsDelete == false || e.IsDelete == null)
            .Include(e => e.Eventtypes) // Lấy luôn thông tin loại sự kiện
            .ToListAsync();
    }

    // GetByID Event
    public async Task<Event> GetEventByIdAsync(int eventId)
    {
        return await _context.Events
            .Where(e => e.EventsId == eventId && (e.IsDelete == false || e.IsDelete == null))
            .Include(e => e.Eventtypes) // Lấy luôn thông tin loại sự kiện
            .FirstOrDefaultAsync();
    }

    //Update Event 
    public async Task<bool> UpdateEventAsync(Event updatedEvent)
    {
        var existingEvent = await _context.Events.FindAsync(updatedEvent.EventsId);
        if (existingEvent == null)
        {
            return false; // Không tìm thấy sự kiện
        }

        // Cập nhật thông tin nếu có giá trị mới, nếu không giữ nguyên giá trị cũ
        existingEvent.EventName = !string.IsNullOrWhiteSpace(updatedEvent.EventName) ? updatedEvent.EventName : existingEvent.EventName;
        existingEvent.EventTime = updatedEvent.EventTime ?? existingEvent.EventTime;
        existingEvent.EndTime = updatedEvent.EndTime ?? existingEvent.EndTime;
        existingEvent.Location = !string.IsNullOrWhiteSpace(updatedEvent.Location) ? updatedEvent.Location : existingEvent.Location;
        existingEvent.Description = !string.IsNullOrWhiteSpace(updatedEvent.Description) ? updatedEvent.Description : existingEvent.Description;
        existingEvent.RequiredParticipants = updatedEvent.RequiredParticipants ?? existingEvent.RequiredParticipants;
        existingEvent.MaxParticipants = updatedEvent.MaxParticipants ?? existingEvent.MaxParticipants;
        existingEvent.UpdateAt = DateTime.Now; // Cập nhật thời gian sửa đổi

        await _context.SaveChangesAsync();
        return true; // Cập nhật thành công
    }

    //Delete
    public async Task<bool> DeleteEventAsync(int eventId)
    {
        var existingEvent = await _context.Events.FindAsync(eventId);
        if (existingEvent == null)
        {
            return false; // Không tìm thấy sự kiện
        }

        // Thay vì xóa hoàn toàn, đặt cờ IsDelete để có thể khôi phục sau này
        existingEvent.IsDelete = true;
        existingEvent.UpdateAt = DateTime.Now; // Ghi lại thời gian cập nhật

        await _context.SaveChangesAsync();
        return true; // Xóa thành công
    }

    // 1️⃣ Lấy danh sách tất cả người dùng (chưa bị xóa)
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsDelete == false || u.IsDelete == null)
            .ToListAsync();
    }

    public async Task<List<Role>> GetRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }


    // Thêm người dùng mới
    public async Task<bool> CreateUserAsync(User user)
    {
        try
        {
            user.CreateAt = DateTime.Now;
            user.IsDelete = false;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    // 2️⃣ Tìm kiếm người dùng theo ID
    public async Task<User> GetUserByIdAsync(int userId)
    {
        return await _context.Users
            .Where(u => u.UserId == userId && (u.IsDelete == false || u.IsDelete == null))
            .Include(u => u.Roles)
            .FirstOrDefaultAsync();
    }


    // 4️⃣ Cập nhật thông tin người dùng
    public async Task<bool> UpdateUserAsync(User updatedUser)
    {
        var existingUser = await _context.Users.FindAsync(updatedUser.UserId);
        if (existingUser == null) return false;

        existingUser.FullName = !string.IsNullOrWhiteSpace(updatedUser.FullName) ? updatedUser.FullName : existingUser.FullName;
        existingUser.Email = !string.IsNullOrWhiteSpace(updatedUser.Email) ? updatedUser.Email : existingUser.Email;
        existingUser.PhoneNumber = !string.IsNullOrWhiteSpace(updatedUser.PhoneNumber) ? updatedUser.PhoneNumber : existingUser.PhoneNumber;
        existingUser.Classid = !string.IsNullOrWhiteSpace(updatedUser.Classid) ? updatedUser.Classid : existingUser.Classid;
        existingUser.Classname = !string.IsNullOrWhiteSpace(updatedUser.Classname) ? updatedUser.Classname : existingUser.Classname;
        existingUser.UpdateAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    // 5️⃣ Xóa mềm người dùng (ẩn đi)
    public async Task<bool> SoftDeleteUserAsync(int userId)
    {
        var existingUser = await _context.Users.FindAsync(userId);
        if (existingUser == null) return false;

        existingUser.IsDelete = true;
        existingUser.UpdateAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    // 6️⃣ Xóa cứng người dùng (Xóa hoàn toàn)
    public async Task<bool> HardDeleteUserAsync(int userId)
    {
        var existingUser = await _context.Users
            .Include(u => u.Eventparticipations)
            .Include(u => u.UserNotifications)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (existingUser == null) return false;

        // Xóa tất cả dữ liệu liên quan trước khi xóa user
        _context.Eventparticipations.RemoveRange(existingUser.Eventparticipations);
        _context.UserNotifications.RemoveRange(existingUser.UserNotifications);

        _context.Users.Remove(existingUser);
        await _context.SaveChangesAsync();
        return true;
    }
    //Update Role & NameClass
    public async Task<bool> UpdateUserRoleAndClassAsync(int userId, int roleId, string classId, string className)
    {
        try
        {
            var existingUser = await _context.Users.FindAsync(userId);
            if (existingUser == null) return false;

            // Kiểm tra roleId có tồn tại không
            var roleExists = await _context.Roles.AnyAsync(r => r.RolesId == roleId);
            if (!roleExists) return false; // Nếu quyền không hợp lệ, không cập nhật

            // Kiểm tra classId & className không rỗng
            if (string.IsNullOrWhiteSpace(classId) || string.IsNullOrWhiteSpace(className))
                return false;

            existingUser.RolesId = roleId;
            existingUser.Classid = classId;
            existingUser.Classname = className;
            existingUser.UpdateAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi cập nhật quyền & lớp học: {ex.Message}");
            return false;
        }
    }

    //Tham gia sự kiện
    //Get All Eventparticipation(tham gia sự kiện)
    public async Task<List<Eventparticipation>> GetAllEventParticipationsAsync()
    {
        return await _context.Eventparticipations
            .Where(ep => ep.IsDelete == false || ep.IsDelete == null)
            .Include(ep => ep.Events)  // Lấy thông tin sự kiện
            .Include(ep => ep.User)    // Lấy thông tin người dùng
            .ToListAsync();
    }

    //Detail Eventparticipation
    public async Task<Eventparticipation> GetEventParticipationByIdAsync(int participationId)
    {
        return await _context.Eventparticipations
            .Where(ep => ep.ParticipationId == participationId)  
            .Include(ep => ep.Events)  
            .Include(ep => ep.User)  
            .FirstOrDefaultAsync();
    }

    //Delete Eventparticipation
    public async Task<bool> DeleteEventparticipationAsync(int eventId)
    {
        var existingEvent = await _context.Eventparticipations.FindAsync(eventId);
        if (existingEvent == null)
        {
            return false; // Không tìm thấy sự kiện
        }

        // Thay vì xóa hoàn toàn, đặt cờ IsDelete để có thể khôi phục sau này
        existingEvent.IsDelete = true;
        existingEvent.UpdateAt = DateTime.Now; // Ghi lại thời gian cập nhật

        await _context.SaveChangesAsync();
        return true; // Xóa thành công
    }

    // Cập nhật thông tin tham gia sự kiện
    public async Task<bool> UpdateEventParticipationAsync(Eventparticipation updatedParticipation)
    {
        var existingParticipation = await _context.Eventparticipations.FindAsync(updatedParticipation.ParticipationId);
        if (existingParticipation == null)
        {
            return false; // Không tìm thấy dữ liệu
        }

        // Cập nhật thông tin nếu có giá trị mới
        existingParticipation.ParticipationStatus = !string.IsNullOrWhiteSpace(updatedParticipation.ParticipationStatus)
            ? updatedParticipation.ParticipationStatus
            : existingParticipation.ParticipationStatus;

        existingParticipation.EarnedPoints = updatedParticipation.EarnedPoints ?? existingParticipation.EarnedPoints;
        existingParticipation.ParticipationTime = updatedParticipation.ParticipationTime ?? existingParticipation.ParticipationTime;
        existingParticipation.UpdateAt = DateTime.Now; // Cập nhật thời gian sửa đổi

        await _context.SaveChangesAsync();
        return true; // Cập nhật thành công
    }

}
