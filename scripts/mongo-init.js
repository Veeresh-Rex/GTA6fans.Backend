// MongoDB initialization script for GTA6fans database

// Switch to the gta6fans database
db = db.getSiblingDB('gta6fans');

// Create collections with validation schemas
db.createCollection('users', {
  validator: {
    $jsonSchema: {
      bsonType: 'object',
      required: ['username', 'email', 'firstName', 'lastName', 'createdAt', 'updatedAt'],
      properties: {
        username: {
          bsonType: 'string',
          description: 'Username must be a string and is required'
        },
        email: {
          bsonType: 'string',
          pattern: '^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$',
          description: 'Email must be a valid email address and is required'
        },
        firstName: {
          bsonType: 'string',
          description: 'First name must be a string and is required'
        },
        lastName: {
          bsonType: 'string',
          description: 'Last name must be a string and is required'
        },
        isActive: {
          bsonType: 'bool',
          description: 'Active status must be a boolean'
        },
        isDeleted: {
          bsonType: 'bool',
          description: 'Deleted status must be a boolean'
        },
        createdAt: {
          bsonType: 'date',
          description: 'Created date must be a date and is required'
        },
        updatedAt: {
          bsonType: 'date',
          description: 'Updated date must be a date and is required'
        },
        lastLoginAt: {
          bsonType: ['date', 'null'],
          description: 'Last login date must be a date or null'
        }
      }
    }
  }
});

// Create indexes for better performance
db.users.createIndex({ 'username': 1 }, { unique: true });
db.users.createIndex({ 'email': 1 }, { unique: true });
db.users.createIndex({ 'isActive': 1 });
db.users.createIndex({ 'isDeleted': 1 });
db.users.createIndex({ 'createdAt': 1 });

// Insert sample data (optional - you can remove this in production)
db.users.insertMany([
  {
    username: 'admin',
    email: 'admin@gta6fans.com',
    firstName: 'Admin',
    lastName: 'User',
    isActive: true,
    isDeleted: false,
    createdAt: new Date(),
    updatedAt: new Date(),
    lastLoginAt: null
  },
  {
    username: 'johndoe',
    email: 'john.doe@example.com',
    firstName: 'John',
    lastName: 'Doe',
    isActive: true,
    isDeleted: false,
    createdAt: new Date(),
    updatedAt: new Date(),
    lastLoginAt: null
  },
  {
    username: 'janesmith',
    email: 'jane.smith@example.com',
    firstName: 'Jane',
    lastName: 'Smith',
    isActive: true,
    isDeleted: false,
    createdAt: new Date(),
    updatedAt: new Date(),
    lastLoginAt: null
  }
]);

print('MongoDB initialization completed successfully!');
print('Database: gta6fans');
print('Collections created: users');
print('Indexes created on: username, email, isActive, isDeleted, createdAt');
print('Sample users inserted: 3');

// Verify the setup
print('Collection stats:');
printjson(db.users.stats());

print('Indexes:');
printjson(db.users.getIndexes());