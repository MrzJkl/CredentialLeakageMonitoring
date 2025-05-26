import 'package:flutter/material.dart';
import 'package:credential_leakage_monitoring/models/leak_model.dart';
import 'package:credential_leakage_monitoring/services/api_service.dart';

class CustomerLeaksScreen extends StatefulWidget {
  const CustomerLeaksScreen({
    super.key,
    required this.customerId,
    required this.customerName,
  });

  final String customerId;
  final String customerName;

  @override
  State<CustomerLeaksScreen> createState() => _CustomerLeaksScreenState();
}

class _CustomerLeaksScreenState extends State<CustomerLeaksScreen> {
  late Future<List<LeakModel>> leaksFuture;

  @override
  void initState() {
    super.initState();
    leaksFuture = ApiService().queryLeaksForCustomer(widget.customerId);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text('Leaks for ${widget.customerName}')),
      body: FutureBuilder<List<LeakModel>>(
        future: leaksFuture,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }
          if (snapshot.hasError) {
            return Center(child: Text('Error: ${snapshot.error}'));
          }
          final leaks = snapshot.data!;
          if (leaks.isEmpty) {
            return const Center(child: Text('No leaks found.'));
          }
          return LeakPaginatedTable(leaks: leaks);
        },
      ),
    );
  }
}

class LeakPaginatedTable extends StatelessWidget {
  const LeakPaginatedTable({super.key, required this.leaks});

  final List<LeakModel> leaks;

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      child: PaginatedDataTable(
        header: Text('Number of Leaks: ${leaks.length}'),
        columns: const [
          DataColumn(label: Text('E-Mail Hash')),
          DataColumn(label: Text('Password Hash')),
          DataColumn(label: Text('Domain')),
          DataColumn(label: Text('First Seen')),
          DataColumn(label: Text('Last Seen')),
        ],
        source: LeakDataSource(leaks),
        rowsPerPage: 20,
        showFirstLastButtons: true,
      ),
    );
  }
}

class LeakDataSource extends DataTableSource {
  LeakDataSource(this.leaks);

  final List<LeakModel> leaks;

  @override
  DataRow? getRow(int index) {
    if (index >= leaks.length) return null;
    final leak = leaks[index];
    return DataRow.byIndex(
      index: index,
      cells: [
        DataCell(Text(leak.emailHash)),
        DataCell(Text(leak.passwordHash)),
        DataCell(Text(leak.domain)),
        DataCell(Text(leak.firstSeen.toString())),
        DataCell(Text(leak.lastSeen.toString())),
      ],
    );
  }

  @override
  bool get isRowCountApproximate => false;

  @override
  int get rowCount => leaks.length;

  @override
  int get selectedRowCount => 0;
}
