# Welcome!

Congratulations on making it this far in the interview process!

Your next step should you choose to accept it is to build a fun web-api for managing a pet shop.

# Pet Shop

Let's pretend you've decided to open up a pet shop and you need to keep track of your sales. You want to know information about your customers as well as what pets they purchased.

The front end will be built later, so for now, you just want to get a web-api set up. Your web-api needs to be built using c# and dotnet core 8.0.

You will need to manage the following:

1. Creating, reading and updating Customer information.

   - At a minimum this should include first name and last name. Feel free to add other information you think is relevant.
   - Reading the Customer record should return an estimated and actual payment due based on their orders.

2. Creating, reading and updating Customer Orders.
   - Orders require a Customer which can only be set when the order is created.
   - Orders also require a Pickup Date which must be today or in the future.
   - Orders should include a Status. Possible Status's can be: 'Open', 'Processing', and 'Delivered'.
   - Orders should default to 'Open' when created.
   - Allow adding any number of pets to an Open order. Each pet should have a name and a price. Feel free to add other details such as Kind (cat, dog, bird, goldfish...) or Color, etc.
   - Orders can be updated when in an Open status, pets can be added or removed and can only be changed to Processing if at least one pet exists on that order.
   - When and order is 'Processing' only the Pickup Date can be edited.
   - When an order is 'Delivered' it can't be updated.
   - When an Order is not 'Delivered', if you read the order it should return an estimated cost based on the price of the pets.
   - Once the order is set to 'Delivered' the actual cost should be saved on the order. Reading the order should return the actual cost based on those items.

Consider how you would structure your api if it was part of a larger project. Feel free to use an in-memory version of the DbContext in your api.  Also consider adding unit tests.


To proceed, please fork this repository and build the api per the requirements above. 

Have fun with it!  When you are ready, create a pull request with your code changes and we'll have a look!
