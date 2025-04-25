import 'package:flutter/material.dart';
import 'package:credential_leakage_monitoring/models/leak_model.dart';
import 'package:credential_leakage_monitoring/services/api_service.dart';

class CustomerLeaksScreen extends StatefulWidget {
  final String customerId;
  final String customerName;

  const CustomerLeaksScreen({
    super.key,
    required this.customerId,
    required this.customerName,
  });

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
  final List<LeakModel> leaks;

  const LeakPaginatedTable({super.key, required this.leaks});

  @override
  Widget build(BuildContext context) {
    return SingleChildScrollView(
      child: PaginatedDataTable(
        header: Text('Number of Leaks: ${leaks.length}'),
        columns: const [
          DataColumn(label: Text('E-Mail Hash')),
          DataColumn(label: Text('Obf. Password')),
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
  final List<LeakModel> leaks;

  LeakDataSource(this.leaks);

  @override
  DataRow? getRow(int index) {
    if (index >= leaks.length) return null;
    final leak = leaks[index];
    return DataRow.byIndex(
      index: index,
      cells: [
        DataCell(Text(leak.emailHash ?? '')),
        DataCell(Text(leak.obfuscatedPassword ?? '')),
        DataCell(Text(leak.domain ?? '')),
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
