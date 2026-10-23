using System.Globalization;
using System.Text.RegularExpressions;

namespace LTNET.Test02.Validation;

public static class MemberValidator
{
    public static string ValidateFullName(string value) =>
        Regex.IsMatch(value.Trim().Normalize(), @"^\p{L}[\p{L}\p{M}]*(?: +\p{L}[\p{L}\p{M}]*)*$")
            ? "" : "Họ tên không được trống; chỉ nhập chữ cái và khoảng trắng.";

    public static string ValidatePhone(string value) =>
        Regex.IsMatch(value, @"\A[0-9]{10}\z")
            ? "" : "Số điện thoại phải gồm đúng 10 chữ số (0–9).";

    public static bool TryParseFee(string value, out long fee) =>
        long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out fee) && fee > 0;

    public static string ValidateFee(string value) => TryParseFee(value, out _)
        ? "" : "Phí dịch vụ phải là số nguyên dương, không có dấu phân cách.";
}
