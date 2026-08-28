namespace NatournaServer.Constants.Error
{
    public static class ErrorCodes
    {
        // Bill Context Manager Error Codes (BILL-xxx)
        public const string BILL_GET_ALL_ERROR = "BILL-001";
        public const string BILL_GET_BY_ID_ERROR = "BILL-002";
        public const string BILL_CREATE_ERROR = "BILL-003";
        public const string BILL_UPDATE_ERROR = "BILL-004";
        public const string BILL_DELETE_ERROR = "BILL-005";

        // Bill API Manager Error Codes (BILL-API-xxx)
        public const string BILL_MARK_AS_PAID_ERROR = "BILL-API-001";
        public const string BILL_MARK_AS_UNPAID_ERROR = "BILL-API-002";
        public const string BILL_NOT_FOUND_ERROR = "BILL-API-003";
        public const string BILL_INSUFFICIENT_BALANCE_ERROR = "BILL-API-004";
        public const string BILL_ALREADY_PAID_ERROR = "BILL-API-005";
        public const string BILL_ALREADY_UNPAID_ERROR = "BILL-API-006";

        // Balance Context Manager Error Codes (BAL-xxx)
        public const string BALANCE_GET_ALL_ERROR = "BAL-001";
        public const string BALANCE_GET_BY_ID_ERROR = "BAL-002";
        public const string BALANCE_CREATE_ERROR = "BAL-003";
        public const string BALANCE_UPDATE_ERROR = "BAL-004";
        public const string BALANCE_DELETE_ERROR = "BAL-005";

        // Balance API Manager Error Codes (BAL-API-xxx)
        public const string BALANCE_NOT_FOUND_ERROR = "BAL-API-001";
        public const string BALANCE_COMPOUND_INVALID_ERROR = "BAL-API-002";

        // Payment Context Manager Error Codes (PAY-xxx)
        public const string PAYMENT_GET_ALL_ERROR = "PAY-001";
        public const string PAYMENT_GET_BY_ID_ERROR = "PAY-002";
        public const string PAYMENT_CREATE_ERROR = "PAY-003";
        public const string PAYMENT_UPDATE_ERROR = "PAY-004";
        public const string PAYMENT_DELETE_ERROR = "PAY-005";

        // Payment API Manager Error Codes (PAY-API-xxx)
        public const string PAYMENT_INVALID_ALLOCATIONS_ERROR = "PAY-API-001";
        public const string PAYMENT_CREATE_WITH_BALANCES_ERROR = "PAY-API-002";
        public const string PAYMENT_MARK_AS_PAID_ERROR = "PAY-API-003";
        public const string PAYMENT_MARK_AS_UNPAID_ERROR = "PAY-API-004";
        public const string PAYMENT_NOT_FOUND_ERROR = "PAY-API-005";
        public const string PAYMENT_ALREADY_PAID_ERROR = "PAY-API-006";
        public const string PAYMENT_ALREADY_UNPAID_ERROR = "PAY-API-007";
        public const string PAYMENT_APARTMENT_INVALID_ERROR = "PAY-API-008";

        // Apartment API Manager Error Codes (APT-API-xxx)
        public const string APARTMENT_BUILDING_INVALID_ERROR = "APT-API-001";

        // Apartment Context Manager Error Codes (APT-xxx)
        public const string APARTMENT_GET_ALL_ERROR = "APT-001";
        public const string APARTMENT_GET_BY_ID_ERROR = "APT-002";
        public const string APARTMENT_CREATE_ERROR = "APT-003";
        public const string APARTMENT_UPDATE_ERROR = "APT-004";
        public const string APARTMENT_DELETE_ERROR = "APT-005";

        // Building API Manager Error Codes (BLD-API-xxx)
        public const string BUILDING_COMPOUND_INVALID_ERROR = "BLD-API-001";

        // Building Context Manager Error Codes (BLD-xxx)
        public const string BUILDING_GET_ALL_ERROR = "BLD-001";
        public const string BUILDING_GET_BY_ID_ERROR = "BLD-002";
        public const string BUILDING_CREATE_ERROR = "BLD-003";
        public const string BUILDING_UPDATE_ERROR = "BLD-004";
        public const string BUILDING_DELETE_ERROR = "BLD-005";

        // Compound Context Manager Error Codes (CMP-xxx)
        public const string COMPOUND_GET_ALL_ERROR = "CMP-001";
        public const string COMPOUND_GET_BY_ID_ERROR = "CMP-002";
        public const string COMPOUND_CREATE_ERROR = "CMP-003";
        public const string COMPOUND_UPDATE_ERROR = "CMP-004";
        public const string COMPOUND_DELETE_ERROR = "CMP-005";

        // Cycle Context Manager Error Codes (CYC-xxx)
        public const string CYCLE_GET_ALL_ERROR = "CYC-001";
        public const string CYCLE_GET_BY_ID_ERROR = "CYC-002";
        public const string CYCLE_CREATE_ERROR = "CYC-003";
        public const string CYCLE_UPDATE_ERROR = "CYC-004";
        public const string CYCLE_DELETE_ERROR = "CYC-005";

        // User API Manager Error Codes (USR-API-xxx)
        public const string USER_EMAIL_EXISTS_ERROR = "USR-API-001";
        public const string USER_ROLE_INVALID_ERROR = "USR-API-002";

        // User Context Manager Error Codes (USR-xxx)
        public const string USER_GET_ALL_ERROR = "USR-001";
        public const string USER_GET_BY_ID_ERROR = "USR-002";
        public const string USER_CREATE_ERROR = "USR-003";
        public const string USER_UPDATE_ERROR = "USR-004";
        public const string USER_DELETE_ERROR = "USR-005";
    }
}
