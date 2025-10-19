using System.Net;
using System.Net.Mail;
using AppTech.Business.Helpers;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Entities.Identity;
using AppTech.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.Services.ExternalServices.Abstractions
{
    public class EmailService : IEmailService
    {
        private readonly IHttpContextAccessor _http;
        public EmailService(IHttpContextAccessor http)
        {
            _http = http;
        }

        public async Task SendMailMessageAsync(string toUser, User user, int confirmationNumber, bool numberOrLink, string token)
        {
            var url = $"https://test.apptech.edu.az/api/accounts/confirm-update-email?userId={user.Id}&token={token}";
            var lang = new LanguageCatcher(_http).GetLanguage();
            var bodyHtmlScript = string.Empty;

            switch (lang)
            {
                case "ru":
                    bodyHtmlScript = numberOrLink ?
                    $@"
                    <!DOCTYPE html>
                    <html lang='ru'>
                      <head>
                        <meta charset='UTF-8' />
                        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                        <title>Код подтверждения</title>
                      </head>
                      <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
                        <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
                          <tr>
                            <td align='center'>
                              <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                                <tr>
                                  <td align='center' style='background-color: #c3e754; padding: 35px 0;'>
                                    <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='' width='200' style='display: block;'/>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 42px 40px 0 40px; text-align: center; font-family: Space Grotesk, Arial, sans-serif; font-weight: 700;'>
                                    <h1 style='margin: 0; font-size: 49px;'>Код подтверждения</h1>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 40px; text-align: center; color: #333333;'>
                                    <img src='https://auth.apptech.edu.az/uploads/illustration.png' alt='' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Уважаемый {user.FullName ?? user.UserName},</p>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Спасибо за регистрацию на сайте AppTech. Для завершения регистрации используйте следующий код подтверждения:</p>
                                    <div style='font-family: Space Grotesk, Arial, sans-serif; font-weight: 700; font-size: 32px; letter-spacing: 4px; margin: 40px 0; color: rgba(0, 0, 0, 0.87); background-color: #f7f7f7; padding: 15px 30px; border-radius: 8px; display: inline-block; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);'>{confirmationNumber}</div>
                                    <div style='font-size: 18px; line-height: 1.8; margin: 20px 0;'>Этот код действителен в течение следующих 24 часов. Если вы не запрашивали этот код, пожалуйста, проигнорируйте это письмо или свяжитесь с нашей службой поддержки.</div>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                                    <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Свяжитесь с нами</h1>
                                    <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                                    <div style='margin: 20px 0;'>
                                      <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                      <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>                               
                                      </a>
                                      <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                    </div>
                                    <div style='background-color: #c3e754; color: #333333; padding: 10px; text-align: center;'>
                                      <p style='margin: 0;'>Copyright © 2024 Apptech | Все права защищены</p>
                                    </div>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                        </table>
                      </body>
                    </html>"
                    :
                    $@"
                    <!DOCTYPE html>
                    <html lang='ru'>
                      <head>
                        <meta charset='UTF-8' />
                        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                        <title>Ссылка подтверждения</title>
                      </head>
                      <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
                        <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
                          <tr>
                            <td align='center'>
                              <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                                <tr>
                                  <td align='center' style='background-color: #c3e754; padding: 35px 0;'>
                                    <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='' width='200' style='display: block;'/>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 42px 40px 0 40px; text-align: center; font-family: Space Grotesk, Arial, sans-serif; font-weight: 700;'>
                                    <h1 style='margin: 0; font-size: 49px;'>Ссылка подтверждения</h1>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 40px; text-align: center; color: #333333;'>
                                    <img src='https://auth.apptech.edu.az/uploads/illustration.png' alt='' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Уважаемый {user.FullName ?? user.UserName},</p>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Вы должны подтвердить свой адрес электронной почты, поскольку информация вашей учетной записи обновляется. Нажмите кнопку ниже, чтобы завершить проверку:</p>
                                    <a href='{url}' style='appearance: none; user-select: none; position: relative; vertical-align: middle; outline-offset: 2px; line-height: 1.2; font-size: 1rem; display: inline-flex; font-weight: 700; cursor: pointer; font-family: Space Grotesk, Arial, sans-serif; align-items: center; justify-content: center; outline: transparent solid 2px; white-space: nowrap; border-radius: 2px; border: 1px solid #101112; transition: all 0.1s ease 0s; padding: 16px 48px; background-color: black; color: white; text-decoration: none;'>
                                        Нажмите здесь
                                    </a>
                                    <div style='font-size: 18px; line-height: 1.8; margin: 20px 0;'>Эта ссылка действительна в течение следующих 24 часов. Если вы не запрашивали этот код
            <div style='font-size: 18px; line-height: 1.8; margin: 20px 0;'>Эта ссылка действительна в течение следующих 24 часов. Если вы не запрашивали эту ссылку, пожалуйста, проигнорируйте это письмо или свяжитесь с нашей службой поддержки.</div>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                                    <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Свяжитесь с нами</h1>
                                    <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                                    <div style='margin: 20px 0;'>
                                      <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                      <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>                               
                                      </a>
                                      <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                    </div>
                                    <div style='background-color: #c3e754; color: #333333; padding: 10px; text-align: center;'>
                                      <p style='margin: 0;'>Copyright © 2024 Apptech | Все права защищены</p>
                                    </div>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                        </table>
                      </body>
                    </html>";
                    break;

                case "en":
                    bodyHtmlScript = numberOrLink ?
                    $@"
                    <!DOCTYPE html>
                    <html lang='en'>
                      <head>
                        <meta charset='UTF-8' />
                        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                        <title>Confirmation Code</title>
                      </head>
                      <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
                        <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
                          <tr>
                            <td align='center'>
                              <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                                <tr>
                                  <td align='center' style='background-color: #c3e754; padding: 35px 0;'>
                                    <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='' width='200' style='display: block;'/>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 42px 40px 0 40px; text-align: center; font-family: Space Grotesk, Arial, sans-serif; font-weight: 700;'>
                                    <h1 style='margin: 0; font-size: 49px;'>Confirmation Code</h1>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 40px; text-align: center; color: #333333;'>
                                    <img src='https://auth.apptech.edu.az/uploads/illustration.png' alt='' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Dear {user.FullName ?? user.UserName},</p>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Thank you for registering on the AppTech site. To complete your registration, please use the following confirmation code:</p>
                                    <div style='font-family: Space Grotesk, Arial, sans-serif; font-weight: 700; font-size: 32px; letter-spacing: 4px; margin: 40px 0; color: rgba(0, 0, 0, 0.87); background-color: #f7f7f7; padding: 15px 30px; border-radius: 8px; display: inline-block; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);'>{confirmationNumber}</div>
                                    <div style='font-size: 18px; line-height: 1.8; margin: 20px 0;'>This code is valid for the next 24 hours. If you did not request this code, please disregard this email or contact our support team.</div>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                                    <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Contact Us</h1>
                                    <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                                    <div style='margin: 20px 0;'>
                                      <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                      <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>                               
                                      </a>
                                      <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                    </div>
                                    <div style='background-color: #c3e754; color: #333333; padding: 10px; text-align: center;'>
                                      <p style='margin: 0;'>Copyright © 2024 Apptech | All Rights Reserved</p>
                                    </div>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                        </table>
                      </body>
                    </html>"
                    :
                    $@"
                    <!DOCTYPE html>
                    <html lang='en'>
                      <head>
                        <meta charset='UTF-8' />
                        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                        <title>Confirmation Link</title>
                      </head>
                      <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
                        <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
                          <tr>
                            <td align='center'>
                              <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                                <tr>
                                  <td align='center' style='background-color: #c3e754; padding: 35px 0;'>
                                    <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='' width='200' style='display: block;'/>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 42px 40px 0 40px; text-align: center; font-family: Space Grotesk, Arial, sans-serif; font-weight: 700;'>
                                    <h1 style='margin: 0; font-size: 49px;'>Confirmation Link</h1>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 40px; text-align: center; color: #333333;'>
                                    <img src='https://auth.apptech.edu.az/uploads/illustration.png' alt='' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Dear {user.FullName ?? user.UserName},</p>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>You must verify your email address as your account information is updated. Click the button below to complete the verification:</p>
                                    <a href='{url}' style='appearance: none; user-select: none; position: relative; vertical-align: middle; outline-offset: 2px; line-height: 1.2; font-size: 1rem; display: inline-flex; font-weight: 700; cursor: pointer; font-family: Space Grotesk, Arial, sans-serif; align-items: center; justify-content: center; outline: transparent solid 2px; white-space: nowrap; border-radius: 2px; border: 1px solid #101112; transition: all 0.1s ease 0s; padding: 16px 48px; background-color: black; color: white; text-decoration: none;'>
                                        Click Here
                                    </a>
                                    <div style='font-size: 18px; line-height: 1.8; margin: 20px 0;'>This link is valid for the next 24 hours. If you did not request this link, please disregard this email or contact our support team.</div>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                                    <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Contact Us</h1>
                                    <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                                    <div style='margin: 20px 0;'>
                                      <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                      <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>                               
                                      </a>
                                      <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                    </div>
                                    <div style='background-color: #c3e754; color: #333333; padding: 10px; text-align: center;'>
                                      <p style='margin: 0;'>Copyright © 2024 Apptech | All Rights Reserved</p>
                                    </div>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                        </table>
                      </body>
                    </html>";
                    break;

                case "az":
                    bodyHtmlScript = numberOrLink ?
                    $@"
                    <!DOCTYPE html>
                    <html lang='az'>
                      <head>
                        <meta charset='UTF-8' />
                        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                        <title>Təsdiq Kodu</title>
                      </head>
                      <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
                        <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
                          <tr>
                            <td align='center'>
                              <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                                <tr>
                                  <td align='center' style='background-color: #c3e754; padding: 35px 0;'>
                                    <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='' width='200' style='display: block;'/>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 42px 40px 0 40px; text-align: center; font-family: Space Grotesk, Arial, sans-serif; font-weight: 700;'>
                                    <h1 style='margin: 0; font-size: 49px;'>Təsdiq Kodu</h1>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 40px; text-align: center; color: #333333;'>
                                    <img src='https://auth.apptech.edu.az/uploads/illustration.png' alt='' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Hörmətli {user.FullName ?? user.UserName},</p>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>AppTech saytından qeydiyyatdan keçdiyiniz üçün təşəkkür edirik. Qeydiyyatınızı tamamlamaq üçün aşağıdakı təsdiq kodundan istifadə edin:</p>
                                    <div style='font-family: Space Grotesk, Arial, sans-serif; font-weight: 700; font-size: 32px; letter-spacing: 4px; margin: 40px 0; color: rgba(0, 0, 0, 0.87); background-color: #f7f7f7; padding: 15px 30px; border-radius: 8px; display: inline-block; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);'>{confirmationNumber}</div>
                                    <div style='font-size: 18px; line-height: 1.8; margin: 20px 0;'>Bu kod növbəti 24 saat ərzində etibarlıdır. Əgər bu kodu siz tələb etməmisinizsə, xahiş edirik bu e-poçtu nəzərə almayın və ya dəstək komandamızla əlaqə saxlayın.</div>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                                    <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Bizimlə Əlaqə</h1>
                                    <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                                    <div style='margin: 20px 0;'>
                                      <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                      <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>                               
                                      </a>
                                      <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                         <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                    </div>
                                    <div style='background-color: #c3e754; color: #333333; padding: 10px; text-align: center;'>
                                      <p style='margin: 0;'>Copyright © 2024 Apptech | All Rights Reserved</p>
                                    </div>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                        </table>
                      </body>
                    </html>"
                    :
                    $@"
                    <!DOCTYPE html>
                    <html lang='az'>
                      <head>
                        <meta charset='UTF-8' />
                        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                        <title>Təsdiq Linki</title>
                      </head>
                      <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
                        <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
                          <tr>
                            <td align='center'>
                              <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                                <tr>
                                  <td align='center' style='background-color: #c3e754; padding: 35px 0;'>
                                    <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='' width='200' style='display: block;'/>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 42px 40px 0 40px; text-align: center; font-family: Space Grotesk, Arial, sans-serif; font-weight: 700;'>
                                    <h1 style='margin: 0; font-size: 49px;'>Təsdiq Linki</h1>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 40px; text-align: center; color: #333333;'>
                                    <img src='https://auth.apptech.edu.az/uploads/illustration.png' alt='' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Hörmətli {user.FullName ?? user.UserName},</p>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Hesab məlumatlarınız yeniləndiyi üçün email ünvanınız doğrulamalısınız. Doğrulanmanı tamamlamaq üçün aşağıdakı düyməyə klikləyin:</p>
                                    <a href='{url}' style='appearance: none; user-select: none; position: relative; vertical-align: middle; outline-offset: 2px; line-height: 1.2; font-size: 1rem; display: inline-flex; font-weight: 700; cursor: pointer; font-family: Space Grotesk, Arial, sans-serif; align-items: center; justify-content: center; outline: transparent solid 2px; white-space: nowrap; border-radius: 2px; border: 1px solid #101112; transition: all 0.1s ease 0s; padding: 16px 48px; background-color: black; color: white; text-decoration: none;'>
                                        Click me
                                    </a>
                                    <div style='font-size: 18px; line-height: 1.8; margin: 20px 0;'>Bu kod növbəti 24 saat ərzində etibarlıdır. Əgər bu kodu siz tələb etməmisinizsə, xahiş edirik bu e-poçtu nəzərə almayın və ya dəstək komandamızla əlaqə saxlayın.</div>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                                    <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Bizimlə Əlaqə</h1>
                                    <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                                    <div style='margin: 20px 0;'>
                                      <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                      <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>                               
                                      </a>
                                      <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                         <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                    </div>
                                    <div style='background-color: #c3e754; color: #333333; padding: 10px; text-align: center;'>
                                      <p style='margin: 0;'>Copyright © 2024 Apptech | All Rights Reserved</p>
                                    </div>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                        </table>
                      </body>
                    </html>";
                    break;

                default:
                    bodyHtmlScript = numberOrLink ?
                    $@"
                    <!DOCTYPE html>
                    <html lang='en'>
                      <head>
                        <meta charset='UTF-8' />
                        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                        <title>Confirmation Code</title>
                      </head>
                      <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
                        <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
                          <tr>
                            <td align='center'>
                              <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                                <tr>
                                  <td align='center' style='background-color: #c3e754; padding: 35px 0;'>
                                    <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='' width='200' style='display: block;'/>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 42px 40px 0 40px; text-align: center; font-family: Space Grotesk, Arial, sans-serif; font-weight: 700;'>
                                    <h1 style='margin: 0; font-size: 49px;'>Confirmation Code</h1>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 40px; text-align: center; color: #333333;'>
                                    <img src='https://auth.apptech.edu.az/uploads/illustration.png' alt='' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Dear {user.FullName ?? user.UserName},</p>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Thank you for registering on the AppTech site. To complete your registration, please use the following confirmation code:</p>
                                    <div style='font-family: Space Grotesk, Arial, sans-serif; font-weight: 700; font-size: 32px; letter-spacing: 4px; margin: 40px 0; color: rgba(0, 0, 0, 0.87); background-color: #f7f7f7; padding: 15px 30px; border-radius: 8px; display: inline-block; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);'>{confirmationNumber}</div>
                                    <div style='font-size: 18px; line-height: 1.8; margin: 20px 0;'>This code is valid for the next 24 hours. If you did not request this code, please disregard this email or contact our support team.</div>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                                    <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Contact Us</h1>
                                    <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                                    <div style='margin: 20px 0;'>
                                      <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                      <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>                               
                                      </a>
                                      <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                    </div>
                                    <div style='background-color: #c3e754; color: #333333; padding: 10px; text-align: center;'>
                                      <p style='margin: 0;'>Copyright © 2024 Apptech | All Rights Reserved</p>
                                    </div>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                        </table>
                      </body>
                    </html>"
                    :
                    $@"
                    <!DOCTYPE html>
                    <html lang='en'>
                      <head>
                        <meta charset='UTF-8' />
                        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                        <title>Confirmation Link</title>
                      </head>
                      <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
                        <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
                          <tr>
                            <td align='center'>
                              <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                                <tr>
                                  <td align='center' style='background-color: #c3e754; padding: 35px 0;'>
                                    <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='' width='200' style='display: block;'/>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 42px 40px 0 40px; text-align: center; font-family: Space Grotesk, Arial, sans-serif; font-weight: 700;'>
                                    <h1 style='margin: 0; font-size: 49px;'>Confirmation Link</h1>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 40px; text-align: center; color: #333333;'>
                                    <img src='https://auth.apptech.edu.az/uploads/illustration.png' alt='' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Dear {user.FullName ?? user.UserName},</p>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>You must verify your email address as your account information is updated. Click the button below to complete the verification:</p>
                                    <a href='{url}' style='appearance: none; user-select: none; position: relative; vertical-align: middle; outline-offset: 2px; line-height: 1.2; font-size: 1rem; display: inline-flex; font-weight: 700; cursor: pointer; font-family: Space Grotesk, Arial, sans-serif; align-items: center; justify-content: center; outline: transparent solid 2px; white-space: nowrap; border-radius: 2px; border: 1px solid #101112; transition: all 0.1s ease 0s; padding: 16px 48px; background-color: black; color: white; text-decoration: none;'>
                                        Click Here
                                    </a>
                                    <div style='font-size: 18px; line-height: 1.8; margin: 20px 0;'>This link is valid for the next 24 hours. If you did not request this link, please disregard this email or contact our support team.</div>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                                    <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Contact Us</h1>
                                    <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                                    <div style='margin: 20px 0;'>
                                      <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                      <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>                               
                                      </a>
                                      <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                    </div>
                                    <div style='background-color: #c3e754; color: #333333; padding: 10px; text-align: center;'>
                                      <p style='margin: 0;'>Copyright © 2024 Apptech | All Rights Reserved</p>
                                    </div>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                        </table>
                      </body>
                    </html>";
                    break;
            }

            using (var client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.UseDefaultCredentials = false;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new NetworkCredential("no-reply@apptech.edu.az", "hello");
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("no-reply@apptech.edu.az"),
                    Subject = lang switch
                    {
                        "ru" => "Добро пожаловать в AppTech", 
                        "az" => "AppTech-ə xoş gəlmisiniz", 
                        "en" or _ => "Welcome to AppTech" 
                    },
                    Body = bodyHtmlScript,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toUser);
                await client.SendMailAsync(mailMessage);
            }
        }

        public async Task SendConfirmationCodeMessageAsync(User currentUser, int number = 0, string toUser = "",
            bool numberOrLink = true, string token = "")
        {
            await SendMailMessageAsync(
                    toUser: !string.IsNullOrEmpty(toUser) ? toUser : currentUser.Email,
                    confirmationNumber: number,
                    user: currentUser,
                    numberOrLink: numberOrLink,
                    token: token
                );
        }

        private string GenerateUnbanHtml(string lang, string userName)
        {
            switch (lang)
            {
                case "ru":
                    return $@"
                    <!DOCTYPE html>
                    <html lang='ru'>
                      <head>
                        <meta charset='UTF-8' />
                        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                        <title>Аккаунт Разблокирован</title>
                      </head>
                      <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
                        <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
                          <tr>
                            <td align='center'>
                              <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                                <tr>
                                  <td align='center' style='background-color: #4CAF50; padding: 35px 0;'>
                                    <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='AppTech' width='200' style='display: block;'/>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 42px 40px 0 40px; text-align: center; font-family: Space Grotesk, Arial, sans-serif; font-weight: 700;'>
                                    <h1 style='margin: 0; font-size: 49px;'>Ваш аккаунт разблокирован!</h1>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='padding: 40px; text-align: center; color: #333333;'>
                                    <img src='https://auth.apptech.edu.az/uploads/illustration.png' alt='' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Уважаемый {userName},</p>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Ваш аккаунт на платформе AppTech был успешно разблокирован. Теперь у вас есть полный доступ к нашему сайту.</p>
                                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Добро пожаловать обратно!</p>
                                  </td>
                                </tr>
                                <tr>
                                  <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                                    <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Свяжитесь с нами</h1>
                                    <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                                    <div style='margin: 20px 0;'>
                                      <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                      <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                      <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                        <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                                      </a>
                                    </div>
                                    <div style='background-color: #4CAF50; color: #333333; padding: 10px; text-align: center;'>
                                      <p style='margin: 0;'>Copyright © 2024 Apptech | Все права защищены</p>
                                    </div>
                                  </td>
                                </tr>
                              </table>
                            </td>
                          </tr>
                        </table>
                      </body>
                    </html>";

                case "az":
                    return $@"
    <!DOCTYPE html>
    <html lang='az'>
      <head>
        <meta charset='UTF-8' />
        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
        <title>Hesabınız Blokdan Çıxarıldı</title>
      </head>
      <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
        <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
          <tr>
            <td align='center'>
              <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                <tr>
                  <td align='center' style='background-color: #4CAF50; padding: 35px 0;'>
                    <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='AppTech' width='200' style='display: block;'/>
                  </td>
                </tr>
                <tr>
                  <td style='padding: 42px 40px 0 40px; text-align: center; font-family: Space Grotesk, Arial, sans-serif; font-weight: 700;'>
                    <h1 style='margin: 0; font-size: 49px;'>Hesabınız blokdan çıxarıldı!</h1>
                  </td>
                </tr>
                <tr>
                  <td style='padding: 40px; text-align: center; color: #333333;'>
                    <img src='https://auth.apptech.edu.az/uploads/illustration.png' alt='' style='max-width: 100%; display: block; margin: 0 auto;'/>
                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Hörmətli {userName},</p>
                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Sizin AppTech hesabınız uğurla blokdan çıxarılmışdır. İndi tam giriş imkanı əldə etdiniz.</p>
                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Təkrar xoş gəldiniz!</p>
                  </td>
                </tr>
                <tr>
                  <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                    <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Bizimlə Əlaqə</h1>
                    <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                    <div style='margin: 20px 0;'>
                      <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                        <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                      </a>
                      <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                        <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>
                      </a>
                      <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                        <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                      </a>
                    </div>
                    <div style='background-color: #4CAF50; color: #333333; padding: 10px; text-align: center;'>
                      <p style='margin: 0;'>Copyright © 2024 Apptech | Bütün hüquqlar qorunur</p>
                    </div>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
        </table>
      </body>
    </html>";

                default:
                    return $@"
    <!DOCTYPE html>
    <html lang='en'>
      <head>
        <meta charset='UTF-8' />
        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
        <title>Your Account is Unbanned</title>
      </head>
      <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
        <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
          <tr>
            <td align='center'>
              <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                <tr>
                  <td align='center' style='background-color: #4CAF50; padding: 35px 0;'>
                    <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='AppTech' width='200' style='display: block;'/>
                  </td>
                </tr>
                <tr>
                  <td style='padding: 42px 40px 0 40px; text-align: center; font-family: Space Grotesk, Arial, sans-serif; font-weight: 700;'>
                    <h1 style='margin: 0; font-size: 49px;'>Your account is unbanned!</h1>
                  </td>
                </tr>
                <tr>
                  <td style='padding: 40px; text-align: center; color: #333333;'>
                    <img src='https://auth.apptech.edu.az/uploads/illustration.png' alt='' style='max-width: 100%; display: block; margin: 0 auto;'/>
                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Dear {userName},</p>
                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Your account on the AppTech platform has been successfully unbanned. You now have full access to our platform again.</p>
                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Welcome back!</p>
                  </td>
                </tr>
                <tr>
                  <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                    <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Contact Us</h1>
                    <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                    <div style='margin: 20px 0;'>
                      <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                        <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                      </a>
                      <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                        <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>
                      </a>
                      <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                        <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                      </a>
                    </div>
                    <div style='background-color: #4CAF50; color: #333333; padding: 10px; text-align: center;'>
                      <p style='margin: 0;'>Copyright © 2024 Apptech | All Rights Reserved</p>
                    </div>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
        </table>
      </body>
    </html>";

            }
        }

        public async Task SendUnbanMailAsync(string toUser, User user)
        {
            var lang = new LanguageCatcher(_http).GetLanguage();
            var userName = user.FullName ?? user.UserName;
            var bodyHtmlScript = GenerateUnbanHtml(lang, userName);

            using (var client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.UseDefaultCredentials = false;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new NetworkCredential("no-reply@apptech.edu.az", "aqns fvso jzvd afow");
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("no-reply@apptech.edu.az"),
                    Subject = "Account Unbanned",
                    Body = bodyHtmlScript,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toUser);
                await client.SendMailAsync(mailMessage);
            }
        }

        private string GeneratePurchaseHtml(string lang, Transaction transaction, User user)
        {
            switch (lang)
            {
                case "ru":
                    return $@"
            <!DOCTYPE html>
            <html lang='ru'>
              <head>
                <meta charset='UTF-8' />
                <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                <title>Код подтверждения</title>
              </head>
              <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
                <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
                  <tr>
                    <td align='center'>
                      <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                        <tr>
                          <td align='center' style='background-color: #c3e754; padding: 35px 0;'>
                            <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='' width='200' style='display: block;'/>
                          </td>
                        </tr>
                        <tr>
                          <td style='padding: 40px;  color: #333333;'>
                          <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Уважаемый(ая) {user.FullName ?? user.UserName},</p>
                          <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'><a href=""https://apptech.edu.az"">https://apptech.edu.az</a> Спасибо за ваш заказ на нашем сайте.</p>
                          <div style='line-height: 30px; margin: 40px 0; color: rgba(0, 0, 0, 0.87); background-color: #f7f7f7; padding: 15px 30px; border-radius: 8px; display: inline-block; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);'>
                            <div style='margin-bottom: 10px;font-family: Space Grotesk, Arial, sans-serif;font-weight: 700; font-size: 24px;'>Ваш заказ:</div>
                            <div><b>ID платежа:</b> {transaction.OrderId}</div>
                            <div><b>Метод оплаты:</b> {transaction.Status}</div>
                            <div><b>Дата заказа:</b> {transaction.CreatedOn.GetValueOrDefault().ToLocalTime()}</div>
                            <div><b>Итого:</b> {transaction.Amount}₼</div>
                          </div>
                        </td>

                        </tr>
                        <tr>
                          <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                            <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Свяжитесь с нами</h1>
                            <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                            <div style='margin: 20px 0;'>
                              <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                              </a>
                              <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>                               
                              </a>
                              <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                              </a>
                            </div>
                            <div style='background-color: #c3e754; color: #333333; padding: 10px; text-align: center;'>
                              <p style='margin: 0;'>Copyright © 2024 Apptech | Все права защищены</p>
                            </div>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                </table>
              </body>
            </html>
            ";

                case "az":
                    return $@"
            <!DOCTYPE html>
            <html lang='az'>
              <head>
                <meta charset='UTF-8' />
                <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                <title>Təsdiq Kodu</title>
              </head>
              <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
                <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
                  <tr>
                    <td align='center'>
                      <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                        <tr>
                          <td align='center' style='background-color: #c3e754; padding: 35px 0;'>
                            <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='' width='200' style='display: block;'/>
                          </td>
                        </tr>
                        <tr>
                          <td style='padding: 40px;  color: #333333;'>
                          <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Hörmətli {user.FullName ?? user.UserName},</p>
                          <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'><a href=""https://apptech.edu.az"">https://apptech.edu.az</a> saytından verdiyiniz sifariş üçün təşəkkür edirik.</p>
                          <div style='line-height: 30px; margin: 40px 0; color: rgba(0, 0, 0, 0.87); background-color: #f7f7f7; padding: 15px 30px; border-radius: 8px; display: inline-block; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);'>
                            <div style='margin-bottom: 10px;font-family: Space Grotesk, Arial, sans-serif;font-weight: 700; font-size: 24px;'>Sifarişiniz:</div>
                            <div><b>Ödəniş ID:</b> {transaction.OrderId}</div>
                            <div><b>Ödəniş üsulu:</b> {transaction.Status}</div>
                            <div><b>Sifariş tarixi:</b> {transaction.CreatedOn.GetValueOrDefault().ToLocalTime()}</div>
                            <div><b>Yekun məbləğ:</b> {transaction.Amount}₼</div>
                          </div>
                        </td>

                        </tr>
                        <tr>
                          <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                            <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Bizimlə Əlaqə</h1>
                            <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                            <div style='margin: 20px 0;'>
                              <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                              </a>
                              <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>                               
                              </a>
                              <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                              </a>
                            </div>
                            <div style='background-color: #c3e754; color: #333333; padding: 10px; text-align: center;'>
                              <p style='margin: 0;'>Müəllif hüquqları © 2024 Apptech | Bütün hüquqlar qorunur</p>
                            </div>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                </table>
              </body>
            </html>
            ";

                default:
                    return $@"
            <!DOCTYPE html>
            <html lang='en'>
              <head>
                <meta charset='UTF-8' />
                <meta name='viewport' content='width=device-width, initial-scale=1.0' />
                <title>Confirmation Code</title>
              </head>
              <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
                <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
                  <tr>
                    <td align='center'>
                      <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                        <tr>
                          <td align='center' style='background-color: #c3e754; padding: 35px 0;'>
                            <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='' width='200' style='display: block;'/>
                          </td>
                        </tr>
                        <tr>
                          <td style='padding: 40px;  color: #333333;'>
                            <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Dear {user.FullName ?? user.UserName},</p>
                            <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'><a href=""https://apptech.edu.az"">https://apptech.edu.az</a> for your order. Thank you for ordering from our website.</p>
                            <div style='line-height: 30px; margin: 40px 0; color: rgba(0, 0, 0, 0.87); background-color: #f7f7f7; padding: 15px 30px; border-radius: 8px; display: inline-block; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);'>
                              <div style='margin-bottom: 10px;font-family: Space Grotesk, Arial, sans-serif;font-weight: 700; font-size: 24px;'>Your order:</div>
                              <div><b>Payment ID:</b> {transaction.OrderId}</div>
                              <div><b>Payment method:</b> {transaction.Status}</div>
                              <div><b>Order date:</b> {transaction.CreatedOn.GetValueOrDefault().ToLocalTime()}</div>
                              <div><b>Grand total:</b> {transaction.Amount}₼</div>
                            </div>
                          </td>
                        </tr>
                        <tr>
                          <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                            <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Contact Us</h1>
                            <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                            <div style='margin: 20px 0;'>
                              <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                              </a>
                              <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>                               
                              </a>
                              <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                                <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                              </a>
                            </div>
                            <div style='background-color: #c3e754; color: #333333; padding: 10px; text-align: center;'>
                              <p style='margin: 0;'>Copyright © 2024 Apptech | All Rights Reserved</p>
                            </div>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                </table>
              </body>
            </html>
            ";
            }
        }

        public void SendPruchaseMail(Transaction transaction, User user)
        {
            var lang = new LanguageCatcher(_http).GetLanguage();
            var bodyHtmlScript = GeneratePurchaseHtml(lang, transaction, user);

            using (var client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.UseDefaultCredentials = false;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new NetworkCredential("no-reply@apptech.edu.az", "aqns fvso jzvd afow");
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("no-reply@apptech.edu.az"),
                    Subject = lang switch
                    {
                        "ru" => "Счет-фактура от вашего аккаунта",
                        "az" => "Hesabınızdan Faktura",
                        "en" or _ => "Invoice from Your Account"
                    },
                    Body = bodyHtmlScript,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(user.Email);

                client.Send(mailMessage);
            }
        }

        private string GenerateRefundHtml(string lang, Transaction transaction, User user)
        {
            switch (lang)
            {
                case "ru":
                    return $@"
    <!DOCTYPE html>
    <html lang='ru'>
      <head>
        <meta charset='UTF-8' />
        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
        <title>Возврат средств</title>
      </head>
      <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
        <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
          <tr>
            <td align='center'>
              <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                <tr>
                  <td align='center' style='background-color: #c3e754; padding: 35px 0;'>
                    <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='' width='200' style='display: block;'/>
                  </td>
                </tr>
                <tr>
                  <td style='padding: 40px; color: #333333;'>
                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Уважаемый(ая) {user.FullName ?? user.UserName},</p>
                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Ваш платеж с ID {transaction.OrderId} был успешно возмещен. Спасибо за использование нашего сервиса.</p>
                    <div style='line-height: 30px; margin: 40px 0; color: rgba(0, 0, 0, 0.87); background-color: #f7f7f7; padding: 15px 30px; border-radius: 8px; display: inline-block; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);'>
                      <div style='margin-bottom: 10px;font-family: Space Grotesk, Arial, sans-serif;font-weight: 700; font-size: 24px;'>Детали возврата:</div>
                      <div><b>ID платежа:</b> {transaction.OrderId}</div>
                      <div><b>Статус:</b> Возвращено</div>
                      <div><b>Дата возврата:</b> {transaction.UpdatedOn.ToLocalTime()}</div>
                      <div><b>Сумма возврата:</b> {transaction.Amount}₼</div>
                    </div>
                  </td>
                </tr>
                <tr>
                  <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                    <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Свяжитесь с нами</h1>
                    <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                    <div style='margin: 20px 0;'>
                      <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                        <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                      </a>
                      <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                        <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>                               
                      </a>
                      <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                        <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                      </a>
                    </div>
                    <div style='background-color: #c3e754; color: #333333; padding: 10px; text-align: center;'>
                      <p style='margin: 0;'>Copyright © 2024 Apptech | Все права защищены</p>
                    </div>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
        </table>
      </body>
    </html>
    ";

                case "az":
                    return $@"
    <!DOCTYPE html>
    <html lang='az'>
      <head>
        <meta charset='UTF-8' />
        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
        <title>Ödənişin Qaytarılması</title>
      </head>
      <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
        <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
          <tr>
            <td align='center'>
              <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                <tr>
                  <td align='center' style='background-color: #c3e754; padding: 35px 0;'>
                    <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='' width='200' style='display: block;'/>
                  </td>
                </tr>
                <tr>
                  <td style='padding: 40px; color: #333333;'>
                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Hörmətli {user.FullName ?? user.UserName},</p>
                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>ID nömrəsi {transaction.OrderId} olan ödənişiniz uğurla geri qaytarıldı. Xidmətimizdən istifadə etdiyiniz üçün təşəkkür edirik.</p>
                    <div style='line-height: 30px; margin: 40px 0; color: rgba(0, 0, 0, 0.87); background-color: #f7f7f7; padding: 15px 30px; border-radius: 8px; display: inline-block; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);'>
                      <div style='margin-bottom: 10px;font-family: Space Grotesk, Arial, sans-serif;font-weight: 700; font-size: 24px;'>Geri Qaytarma Məlumatları:</div>
                      <div><b>Ödəniş ID:</b> {transaction.OrderId}</div>
                      <div><b>Status:</b> Geri Qaytarıldı</div>
                      <div><b>Qaytarılma Tarixi:</b> {transaction.UpdatedOn.ToLocalTime()}</div>
                      <div><b>Qaytarılan Məbləğ:</b> {transaction.Amount}₼</div>
                    </div>
                  </td>
                </tr>
                <tr>
                  <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                    <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Bizimlə Əlaqə</h1>
                    <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                    <div style='margin: 20px 0;'>
                      <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                        <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                      </a>
                      <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                        <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>                               
                      </a>
                      <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                        <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                      </a>
                    </div>
                    <div style='background-color: #c3e754; color: #333333; padding: 10px; text-align: center;'>
                      <p style='margin: 0;'>Müəllif hüquqları © 2024 Apptech | Bütün hüquqlar qorunur</p>
                    </div>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
        </table>
      </body>
    </html>
    ";

                default:
                    return $@"
    <!DOCTYPE html>
    <html lang='en'>
      <head>
        <meta charset='UTF-8' />
        <meta name='viewport' content='width=device-width, initial-scale=1.0' />
        <title>Refund Confirmation</title>
      </head>
      <body style='font-family: DM Sans, Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; box-sizing: border-box;'>
        <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='border-collapse: collapse;'>
          <tr>
            <td align='center'>
              <table role='presentation' width='100%' cellspacing='0' cellpadding='0' border='0' style='background-color: #ffffff; border-radius: 12px; box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1); border: 1px solid #e0e0e0; max-width: 600px; width: 100%;'>
                <tr>
                  <td align='center' style='background-color: #c3e754; padding: 35px 0;'>
                    <img src='https://auth.apptech.edu.az/uploads/logo.png' alt='' width='200' style='display: block;'/>
                  </td>
                </tr>
                <tr>
                  <td style='padding: 40px; color: #333333;'>
                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Dear {user.FullName ?? user.UserName},</p>
                    <p style='font-size: 23px; line-height: 1.8; margin: 20px 0;'>Your payment with ID {transaction.OrderId} has been successfully refunded. Thank you for using our service.</p>
                    <div style='line-height: 30px; margin: 40px 0; color: rgba(0, 0, 0, 0.87); background-color: #f7f7f7; padding: 15px 30px; border-radius: 8px; display: inline-block; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.05);'>
                      <div style='margin-bottom: 10px;font-family: Space Grotesk, Arial, sans-serif;font-weight: 700; font-size: 24px;'>Refund Details:</div>
                      <div><b>Payment ID:</b> {transaction.OrderId}</div>
                      <div><b>Status:</b> Refunded</div>
                      <div><b>Refund Date:</b> {transaction.UpdatedOn.ToLocalTime()}</div>
                      <div><b>Refund Amount:</b> {transaction.Amount}₼</div>
                    </div>
                  </td>
                </tr>
                <tr>
                  <td style='text-align: center; background-color: #191b1e; color: #ffffff; font-size: 14px; font-family: DM Sans, Arial, sans-serif; font-weight: 400;'>
                    <h1 style='padding: 50px 10px 5px; font-size: 34px;'>Contact Us</h1>
                    <hr style='width: 380px; color: #ffffff; border: none; border-top: 1px solid #ffffff; margin: 0 auto;' />
                    <div style='margin: 20px 0;'>
                      <a href='https://www.instagram.com/apptech.edu.az/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                        <img src='https://auth.apptech.edu.az/uploads/instagram.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                      </a>
                      <a href='https://www.linkedin.com/company/apptechmmc/' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                        <img src='https://auth.apptech.edu.az/uploads/linkedin.svg' style='max-width: 100%; display: block; margin: 0 auto;'/>                               
                      </a>
                      <a href='https://wa.me/0504044103' target='_blank' style='display: inline-block; margin: 0 10px; color: #ffffff; text-decoration: none;'>
                        <img src='https://auth.apptech.edu.az/uploads/whatsapp.png' style='max-width: 100%; display: block; margin: 0 auto;'/>
                      </a>
                    </div>
                    <div style='background-color: #c3e754; color: #333333; padding: 10px; text-align: center;'>
                      <p style='margin: 0;'>Copyright © 2024 Apptech | All Rights Reserved</p>
                    </div>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
        </table>
      </body>
    </html>
    ";
            }
        }

        public void SendRefundMail(Transaction transaction, User user)
        {
            var lang = new LanguageCatcher(_http).GetLanguage();
            var bodyHtmlScript = GenerateRefundHtml(lang, transaction, user);

            using (var client = new SmtpClient("smtp.gmail.com", 587))
            {
                client.UseDefaultCredentials = false;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new NetworkCredential("no-reply@apptech.edu.az", "aqns fvso jzvd afow");
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("no-reply@apptech.edu.az"),
                    Subject = lang switch
                    {
                        "ru" => "Возврат средств с вашего аккаунта",
                        "az" => "Hesabınızdan ödənişin qaytarılması",
                        "en" or _ => "Refund from Your Account"
                    },
                    Body = bodyHtmlScript,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(user.Email);

                client.Send(mailMessage);
            }
        }

    }
}
