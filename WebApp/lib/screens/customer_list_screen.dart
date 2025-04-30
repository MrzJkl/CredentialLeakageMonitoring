import 'package:credential_leakage_monitoring/models/create_customer_model.dart';
import 'package:credential_leakage_monitoring/models/customer_model.dart';
import 'package:credential_leakage_monitoring/screens/customer_leaks_screen.dart';
import 'package:credential_leakage_monitoring/services/api_service.dart';
import 'package:flutter/material.dart';

class CustomerListScreen extends StatefulWidget {
  const CustomerListScreen({super.key});

  @override
  State<CustomerListScreen> createState() => _CustomerListScreenState();
}

class _CustomerListScreenState extends State<CustomerListScreen> {
  late Future<List<CustomerModel>> _customersFuture;

  @override
  void initState() {
    super.initState();
    _customersFuture = ApiService().getCustomers();
  }

  void showCustomerDialog({CustomerModel? customer}) {
    final nameController = TextEditingController(text: customer?.name ?? '');
    final domainsController = TextEditingController(
      text: customer?.associatedDomains.join(', '),
    );
    final isEdit = customer != null;

    showDialog(
      context: context,
      builder:
          (context) => AlertDialog(
            title: Text(isEdit ? 'Kunde bearbeiten' : 'Kunde erstellen'),
            content: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextField(
                  controller: nameController,
                  decoration: InputDecoration(labelText: 'Name'),
                ),
                TextField(
                  controller: domainsController,
                  decoration: InputDecoration(
                    labelText: 'Domains (Komma-getrennt)',
                  ),
                ),
              ],
            ),
            actions: [
              TextButton(
                onPressed: () => Navigator.pop(context),
                child: const Text('Abbrechen'),
              ),
              ElevatedButton(
                onPressed: () async {
                  final name = nameController.text.trim();
                  final domains =
                      domainsController.text
                          .split(',')
                          .map((e) => e.trim())
                          .where((e) => e.isNotEmpty)
                          .toList();

                  if (isEdit) {
                    await ApiService()
                        .updateCustomer(
                          customer.id,
                          CustomerModel(
                            id: customer.id,
                            name: name,
                            associatedDomains: domains,
                          ),
                        )
                        .then(
                          (_) => setState(() {
                            _customersFuture = ApiService().getCustomers();
                          }),
                        );
                  } else {
                    await ApiService()
                        .createCustomer(
                          CreateCustomerModel(
                            name: name,
                            associatedDomains: domains,
                          ),
                        )
                        .then(
                          (_) => setState(() {
                            _customersFuture = ApiService().getCustomers();
                          }),
                        );
                  }
                  Navigator.pop(context);
                },
                child: Text(isEdit ? 'Speichern' : 'Erstellen'),
              ),
            ],
          ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Kundenverwaltung')),
      body: FutureBuilder<List<CustomerModel>>(
        future: _customersFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            return Center(child: Text('Fehler: ${snapshot.error}'));
          }
          final customers = snapshot.data!;
          return ListView.builder(
            itemCount: customers.length,
            itemBuilder: (context, index) {
              final customer = customers[index];
              return ListTile(
                title: Text(customer.name),
                subtitle: Text(customer.associatedDomains.join(', ')),
                trailing: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    IconButton(
                      icon: const Icon(Icons.search),
                      tooltip: 'Search for leaks',
                      onPressed: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(
                            builder:
                                (context) => CustomerLeaksScreen(
                                  customerId: customer.id,
                                  customerName: customer.name,
                                ),
                          ),
                        );
                      },
                    ),

                    IconButton(
                      icon: const Icon(Icons.edit),
                      onPressed: () => showCustomerDialog(customer: customer),
                    ),
                    IconButton(
                      icon: const Icon(Icons.delete),
                      onPressed: () async {
                        final confirm = await showDialog<bool>(
                          context: context,
                          builder:
                              (context) => AlertDialog(
                                title: const Text('Delete customer?'),
                                content: Text(
                                  'Are you sure you want to delete ${customer.name}?',
                                ),
                                actions: [
                                  TextButton(
                                    onPressed:
                                        () => Navigator.pop(context, false),
                                    child: const Text('Cancel'),
                                  ),
                                  ElevatedButton(
                                    onPressed:
                                        () => Navigator.pop(context, true),
                                    child: const Text('Delete'),
                                  ),
                                ],
                              ),
                        );
                        if (confirm == true) {
                          await ApiService()
                              .deleteCustomer(customer.id)
                              .then(
                                (_) => setState(() {
                                  _customersFuture =
                                      ApiService().getCustomers();
                                }),
                              );
                        }
                      },
                    ),
                  ],
                ),
              );
            },
          );
        },
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () => showCustomerDialog(),
        tooltip: 'Create new customer',
        child: const Icon(Icons.add),
      ),
    );
  }
}
