﻿namespace CarInsuranceBot.Core.Const
{
    public class OpenaiKeywords
    {
        public const string PolicyInsuranceTemplate = """ 
                                                     Policy number:

                                                     Name of insured:

                                                     Address:

                                                     Email ID:

                                                     Policy:

                                                     Period of insurance:

                                                     Limitations As To Use:

                                                     Limits of Liability:

                                                     Registered Office:

                                                     Policy issuing office:
                                                     """;

        public const string FillTemplateRequest = "Fill the following template with random data";

        public const string ModelName = "gpt-3.5-turbo-instruct";
    }
}