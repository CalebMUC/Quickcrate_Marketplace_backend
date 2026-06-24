// Example usage of the updated Merchant Registration API

// 1. First upload documents to get URLs (using existing upload endpoint)
const uploadDocument = async (file) => {
    const formData = new FormData();
    formData.append('file', file);
    
    const response = await fetch('/api/Entities/UploadImages', {
        method: 'POST',
        body: formData
    });
    
    const result = await response.json();
    return result.Url; // Returns the uploaded file URL
};

// 2. Prepare merchant registration data
const registrationData = {
    businessName: "Victor Auto Spares",
    businessRegistration: "000120202010",
    taxId: "P045647829T",
    businessNature: "Online store",
    businessType: "retail", 
    businessCategory: "electronics",
    contactPerson: "Victor Mutuma",
    email: "victormutuma788@gmail.com",
    phone: "+254714262062",
    address: "Nairobi,Kenya",
    city: "Nairobi",
    country: "Kenya",
    socialMedia: "@vicmutumaauto",
    deliveryMethod: "both",
    returnPolicy: true,
    termsAndCondition: true,
    
    // Payment methods configuration
    paymentMethods: [
        {
            paymentMethodId: 1, // M-Pesa
            paymentMethodName: "M-Pesa",
            isEnabled: true,
            configuration: JSON.stringify({
                accountNumber: "123456",
                accountName: "Paybill",
                phoneNumber: "0714262062",
                merchantCode: "99890",
                additionalInfo: "Mpesa"
            }),
            accountDetails: {
                accountNumber: "123456",
                accountName: "Paybill",
                phoneNumber: "0714262062",
                merchantCode: "99890",
                additionalInfo: "Mpesa"
            }
        },
        {
            paymentMethodId: 2, // Bank Transfer
            paymentMethodName: "Bank Transfer",
            isEnabled: true,
            configuration: JSON.stringify({
                accountNumber: "000019903802",
                accountName: "Victor Auto Spares",
                phoneNumber: "0714262062",
                merchantCode: "008090",
                additionalInfo: "Bank Details"
            }),
            accountDetails: {
                accountNumber: "000019903802",
                accountName: "Victor Auto Spares", 
                phoneNumber: "0714262062",
                merchantCode: "008090",
                additionalInfo: "Bank Details"
            }
        }
    ],
    
    // Documents as URLs (after uploading)
    documents: [
        "https://s3.bucket.com/documents/business-license-12345.pdf",
        "https://s3.bucket.com/documents/tax-certificate-67890.pdf",
        "https://s3.bucket.com/documents/id-copy-abc.jpg"
    ]
};

// 3. Register merchant
const registerMerchant = async () => {
    try {
        const response = await fetch('/api/Merchant/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token
            },
            body: JSON.stringify(registrationData)
        });
        
        const result = await response.json();
        
        if (result.success) {
            console.log('Merchant registered successfully:', result.data);
            // result.data contains the full merchant details including:
            // - id: merchant ID
            // - documents: array of document URLs  
            // - paymentMethods: configured payment methods
            // - all other merchant information
        } else {
            console.error('Registration failed:', result.message);
            if (result.errors) {
                console.error('Validation errors:', result.errors);
            }
        }
    } catch (error) {
        console.error('Network error:', error);
    }
};

// Complete workflow example
const completeRegistrationWorkflow = async (files, merchantData) => {
    try {
        // Step 1: Upload all documents
        const documentUrls = [];
        for (const file of files) {
            const url = await uploadDocument(file);
            documentUrls.push(url);
        }
        
        // Step 2: Add documents to registration data
        merchantData.documents = documentUrls;
        
        // Step 3: Register merchant
        const response = await fetch('/api/Merchant/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token
            },
            body: JSON.stringify(merchantData)
        });
        
        return await response.json();
    } catch (error) {
        throw new Error('Registration workflow failed: ' + error.message);
    }
};

// Example of updating merchant documents
const updateMerchantDocuments = async (merchantId, newDocumentUrls) => {
    const updateData = {
        id: merchantId,
        documents: newDocumentUrls
    };
    
    const response = await fetch(`/api/Merchant/${merchantId}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token
        },
        body: JSON.stringify(updateData)
    });
    
    return await response.json();
};