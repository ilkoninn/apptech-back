using Microsoft.AspNetCore.Identity;

namespace AppTech.Core.Exceptions.IdentityErrors
{
    public class CustomIdentityErrorDescriber : IdentityErrorDescriber
    {
        private string _lang;

        public void SetLanguage(string lang)
        {
            _lang = lang;
        }

        public override IdentityError DefaultError()
        {
            var description = _lang switch
            {
                "ru" => "Произошла неизвестная ошибка.",
                "az" => "Naməlum nasazlıq baş verdi.",
                _ => "An unknown failure has occurred.",
            };

            return new IdentityError
            {
                Code = nameof(DefaultError),
                Description = description
            };
        }

        public override IdentityError ConcurrencyFailure()
        {
            var description = _lang switch
            {
                "ru" => "Ошибка оптимистичного параллелизма, объект был изменен.",
                "az" => "Optimist paralellik xətası, obyekt dəyişdirildi.",
                _ => "Optimistic concurrency failure, object has been modified.",
            };

            return new IdentityError
            {
                Code = nameof(ConcurrencyFailure),
                Description = description
            };
        }

        public override IdentityError PasswordMismatch()
        {
            var description = _lang switch
            {
                "ru" => "Неверный пароль.",
                "az" => "Yanlış şifrə.",
                _ => "Incorrect password.",
            };

            return new IdentityError
            {
                Code = nameof(PasswordMismatch),
                Description = description
            };
        }

        public override IdentityError InvalidToken()
        {
            var description = _lang switch
            {
                "ru" => "Неверный токен.",
                "az" => "Yanlış token.",
                _ => "Invalid token.",
            };

            return new IdentityError
            {
                Code = nameof(InvalidToken),
                Description = description
            };
        }

        public override IdentityError LoginAlreadyAssociated()
        {
            var description = _lang switch
            {
                "ru" => "Пользователь с таким логином уже существует.",
                "az" => "Bu giriş ilə istifadəçi artıq mövcuddur.",
                _ => "A user with this login already exists.",
            };

            return new IdentityError
            {
                Code = nameof(LoginAlreadyAssociated),
                Description = description
            };
        }

        public override IdentityError InvalidUserName(string userName)
        {
            var description = _lang switch
            {
                "ru" => $"Имя пользователя '{userName}' недопустимо, может содержать только буквы или цифры.",
                "az" => $"İstifadəçi adı '{userName}' yanlışdır, yalnız hərflər və rəqəmlər ola bilər.",
                _ => $"User name '{userName}' is invalid, can only contain letters or digits.",
            };

            return new IdentityError
            {
                Code = nameof(InvalidUserName),
                Description = description
            };
        }

        public override IdentityError InvalidEmail(string email)
        {
            var description = _lang switch
            {
                "ru" => $"Электронная почта '{email}' недопустима.",
                "az" => $"Email '{email}' yanlışdır.",
                _ => $"Email '{email}' is invalid.",
            };

            return new IdentityError
            {
                Code = nameof(InvalidEmail),
                Description = description
            };
        }

        public override IdentityError DuplicateUserName(string userName)
        {
            var description = _lang switch
            {
                "ru" => $"Имя пользователя '{userName}' уже занято.",
                "az" => $"İstifadəçi adı '{userName}' artıq götürülüb.",
                _ => $"Username '{userName}' is already taken.",
            };

            return new IdentityError
            {
                Code = nameof(DuplicateUserName),
                Description = description
            };
        }

        public override IdentityError DuplicateEmail(string email)
        {
            var description = _lang switch
            {
                "ru" => $"Электронная почта '{email}' уже используется.",
                "az" => $"Email '{email}' artıq götürülüb.",
                _ => $"Email '{email}' is already taken.",
            };

            return new IdentityError
            {
                Code = nameof(DuplicateEmail),
                Description = description
            };
        }

        public override IdentityError InvalidRoleName(string role)
        {
            var description = _lang switch
            {
                "ru" => $"Имя роли '{role}' недопустимо.",
                "az" => $"Rol adı '{role}' yanlışdır.",
                _ => $"Role name '{role}' is invalid.",
            };

            return new IdentityError
            {
                Code = nameof(InvalidRoleName),
                Description = description
            };
        }

        public override IdentityError DuplicateRoleName(string role)
        {
            var description = _lang switch
            {
                "ru" => $"Имя роли '{role}' уже занято.",
                "az" => $"Rol adı '{role}' artıq götürülüb.",
                _ => $"Role name '{role}' is already taken.",
            };

            return new IdentityError
            {
                Code = nameof(DuplicateRoleName),
                Description = description
            };
        }

        public override IdentityError UserAlreadyHasPassword()
        {
            var description = _lang switch
            {
                "ru" => "У пользователя уже установлен пароль.",
                "az" => "İstifadəçinin artıq şifrəsi var.",
                _ => "User already has a password set.",
            };

            return new IdentityError
            {
                Code = nameof(UserAlreadyHasPassword),
                Description = description
            };
        }

        public override IdentityError UserLockoutNotEnabled()
        {
            var description = _lang switch
            {
                "ru" => "Для этого пользователя блокировка не включена.",
                "az" => "Bu istifadəçi üçün bloklama aktiv deyil.",
                _ => "Lockout is not enabled for this user.",
            };

            return new IdentityError
            {
                Code = nameof(UserLockoutNotEnabled),
                Description = description
            };
        }

        public override IdentityError UserAlreadyInRole(string role)
        {
            var description = _lang switch
            {
                "ru" => $"Пользователь уже в роли '{role}'.",
                "az" => $"İstifadəçi artıq '{role}' rolundadır.",
                _ => $"User already in role '{role}'.",
            };

            return new IdentityError
            {
                Code = nameof(UserAlreadyInRole),
                Description = description
            };
        }

        public override IdentityError UserNotInRole(string role)
        {
            var description = _lang switch
            {
                "ru" => $"Пользователь не в роли '{role}'.",
                "az" => $"İstifadəçi '{role}' rolunda deyil.",
                _ => $"User is not in role '{role}'.",
            };

            return new IdentityError
            {
                Code = nameof(UserNotInRole),
                Description = description
            };
        }

        public override IdentityError PasswordTooShort(int length)
        {
            var description = _lang switch
            {
                "ru" => $"Пароли должны содержать не менее {length} символов.",
                "az" => $"Şifrələr ən azı {length} simvol olmalıdır.",
                _ => $"Passwords must be at least {length} characters.",
            };

            return new IdentityError
            {
                Code = nameof(PasswordTooShort),
                Description = description
            };
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            var description = _lang switch
            {
                "ru" => "Пароли должны содержать хотя бы один не буквенно-цифровой символ.",
                "az" => "Şifrələrdə ən azı bir əlifba-rəqəm olmayan simvol olmalıdır.",
                _ => "Passwords must have at least one non alphanumeric character.",
            };

            return new IdentityError
            {
                Code = nameof(PasswordRequiresNonAlphanumeric),
                Description = description
            };
        }

        public override IdentityError PasswordRequiresDigit()
        {
            var description = _lang switch
            {
                "ru" => "Пароли должны содержать хотя бы одну цифру ('0'-'9').",
                "az" => "Şifrələrdə ən azı bir rəqəm ('0'-'9') olmalıdır.",
                _ => "Passwords must have at least one digit ('0'-'9').",
            };

            return new IdentityError
            {
                Code = nameof(PasswordRequiresDigit),
                Description = description
            };
        }

        public override IdentityError PasswordRequiresLower()
        {
            var description = _lang switch
            {
                "ru" => "Пароли должны содержать хотя бы одну строчную букву ('a'-'z').",
                "az" => "Şifrələrdə ən azı bir kiçik hərf ('a'-'z') olmalıdır.",
                _ => "Passwords must have at least one lowercase ('a'-'z').",
            };

            return new IdentityError
            {
                Code = nameof(PasswordRequiresLower),
                Description = description
            };
        }

        public override IdentityError PasswordRequiresUpper()
        {
            var description = _lang switch
            {
                "ru" => "Пароли должны содержать хотя бы одну заглавную букву ('A'-'Z').",
                "az" => "Şifrələrdə ən azı bir böyük hərf ('A'-'Z') olmalıdır.",
                _ => "Passwords must have at least one uppercase ('A'-'Z').",
            };

            return new IdentityError
            {
                Code = nameof(PasswordRequiresUpper),
                Description = description
            };
        }
    }
}
