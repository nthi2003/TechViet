using System.ComponentModel.DataAnnotations;

namespace TechecomViet.Validate
{
    public class RequiredFieldAttribute : RequiredAttribute
    {
        public RequiredFieldAttribute()
        {
            ErrorMessage = "Trường này không được để trống!";
        }
    }

    public class PositiveNumberAttribute : ValidationAttribute
    {
        public PositiveNumberAttribute()
        {
            ErrorMessage = "Giá trị phải lớn hơn 0!";
        }

        public override bool IsValid(object value)
        {
            if (value == null) return false;
            return int.TryParse(value.ToString(), out int result) && result > 0;
        }
    }

    public class ValidPercentageAttribute : ValidationAttribute
    {
        public ValidPercentageAttribute()
        {
            ErrorMessage = "Phần trăm giảm giá phải từ 0 - 100!";
        }

        public override bool IsValid(object value)
        {
            if (value == null) return true; // Cho phép null (nullable field)
            return int.TryParse(value.ToString(), out int result) && result >= 0 && result <= 100;
        }
    }

    public class MinImageRequiredAttribute : ValidationAttribute
    {
        public MinImageRequiredAttribute()
        {
            ErrorMessage = "Cần ít nhất 1 ảnh sản phẩm!";
        }

        public override bool IsValid(object value)
        {
            var list = value as List<IFormFile>;
            return list != null && list.Count > 0;
        }
    }
}

